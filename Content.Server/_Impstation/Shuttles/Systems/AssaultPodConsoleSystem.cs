using System.Linq;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.NukeOps;
using Content.Server.Pinpointer;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Stack;
using Content.Server.Station.Systems;
using Content.Shared._Impstation.Shuttles.Components;
using Content.Shared._Impstation.Shuttles.Events;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Pinpointer;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Impstation.Shuttles.Systems
{
    public sealed class AssaultPodConsoleSystem : EntitySystem
    {
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly LockSystem _lockSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly MapSystem _mapSystem = default!;
        [Dependency] private readonly NavMapSystem _navMap = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ShuttleSystem _shuttle = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        private static readonly ProtoId<StackPrototype> TelecrystalStackPrototype = "Telecrystal";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AssaultPodConsoleComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<AssaultPodConsoleComponent, DestructionEventArgs>(OnDestruction);
            SubscribeLocalEvent<WarDeclaredEvent>(OnWarDeclared);
            Subs.BuiEvents<AssaultPodConsoleComponent>(StationMapUiKey.Key, subs =>
            {
                subs.Event<ClickCoordMessage>(OnClickCoord);
            });
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<AssaultPodConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                if (comp.LaunchTime == null || comp.Launched || comp.WarDeclared)
                    continue;

                if (comp.LaunchTime > _timing.CurTime)
                    continue;

                comp.Launched = true;

                var shuttleUid = Transform(uid).GridUid;
                if (!TryComp(shuttleUid, out ShuttleComponent? shuttleComp))
                    continue;

                _shuttle.FTLToCoordinates(
                    shuttleUid.Value,
                    shuttleComp,
                    comp.TravelCoordinates,
                    Angle.Zero,
                    hyperspaceTime: comp.TravelTime,
                    globalTravelSound: true,
                    destroyFloor: true,
                    arrivalKnockRadius: comp.ArrivalKnockRadius,
                    travelSound: comp.TravelSound,
                    globalArrivalSound: comp.ArrivalSound);

                if (!TryFindNukeOpsRule(out var nukeopsRule)
                    || nukeopsRule?.TargetStation is not { } targetStation)
                    continue;

                var destinationMapCoords = _transform.ToMapCoordinates(comp.TravelCoordinates);
                var beacon = _navMap.GetNearestBeaconString(destinationMapCoords, true);
                _alertLevelSystem.SetLevel(targetStation, comp.AlertLevel, false, true, true);
                _chat.DispatchFilteredAnnouncement(
                    Filter.BroadcastMap(destinationMapCoords.MapId),
                    Loc.GetString(comp.DepartureStationAnnouncement, ("beacon", beacon)),
                    uid,
                    Loc.GetString(comp.StationAnnouncementSender),
                    announcementSound: comp.DepartureAnnouncementSound,
                    colorOverride: Color.Cyan
                );
            }
        }

        private void OnInteractUsing(Entity<AssaultPodConsoleComponent> ent, ref InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (ent.Comp.CostPayed || ent.Comp.WarDeclared)
                return;

            if (!TryComp<StackComponent>(args.Used, out var stack) || stack.StackTypeId != TelecrystalStackPrototype)
                return;

            var inserted = 0;
            while (inserted < stack.Count)
            {
                inserted++;
                ent.Comp.InsertedTelecrystals++;

                if (ent.Comp.InsertedTelecrystals >= ent.Comp.Cost)
                {
                    ent.Comp.CostPayed = true;

                    var shuttleUid = Transform(ent).GridUid;
                    if (shuttleUid is { } shuttle)
                    {
                        // prevent triggering warops
                        var ev = new ConsoleFTLAttemptEvent(shuttle, false, string.Empty);
                        RaiseLocalEvent(shuttle, ref ev, true);
                    }

                    _lockSystem.Unlock(ent, args.User);
                    break;
                }
            }

            _stackSystem.ReduceCount((args.Used, stack), inserted);
            _lockSystem.SetCustomLockText(ent, Loc.GetString(ent.Comp.LockExamineText, ("telecrystals", ent.Comp.Cost - ent.Comp.InsertedTelecrystals)));

            args.Handled = true;
        }

        private void OnClickCoord(Entity<AssaultPodConsoleComponent> ent, ref ClickCoordMessage args)
        {
            if (ent.Comp.Launched || ent.Comp.LaunchTime != null)
                return;

            if (!_mapMan.TryFindGridAt(args.Coordinates.MapId, args.Coordinates.Position, out var grid, out var gridComp))
                return;

            // prevent accidently sending coords of a debris or shuttle
            if (!TryFindNukeOpsRule(out var nukeopsRule)
                || nukeopsRule?.TargetStation is not { } targetStation
                || _station.GetLargestGrid(targetStation) != grid)
                return;

            var tileRefs = _mapSystem.GetTilesIntersecting(
                grid,
                gridComp,
                new Circle(args.Coordinates.Position, ent.Comp.LandingVariationRange)
                ).ToList();

            if (tileRefs.Count == 0)
                return;

            var chosenTile = _random.Pick(tileRefs);
            var gridEntityCoords = _mapSystem.ToCoordinates(chosenTile, gridComp);
            // conversion to prevent dock ftl from triggering
            ent.Comp.TravelCoordinates = _transform.WithEntityId(gridEntityCoords, _mapSystem.GetMap(args.Coordinates.MapId));
            ent.Comp.LaunchTime = _timing.CurTime + ent.Comp.TimeTillLaunch;

            var beacon = _navMap.GetNearestBeaconString(_transform.ToMapCoordinates(ent.Comp.TravelCoordinates), true);
            _chat.DispatchFilteredAnnouncement(
                Filter.BroadcastMap(Transform(ent).MapID),
                Loc.GetString(ent.Comp.BeginDepartureTimerAnnouncement, ("beacon", beacon)),
                sender: Loc.GetString(ent.Comp.NukieAnnouncementSender),
                announcementSound: ent.Comp.BeginDepartureTimerAnnouncementSound,
                colorOverride: Color.DarkRed
            );
        }

        private void OnDestruction(Entity<AssaultPodConsoleComponent> ent, ref DestructionEventArgs args)
        {
            if (ent.Comp.Launched)
                return;

            _stackSystem.SpawnNextToOrDrop(ent.Comp.InsertedTelecrystals, TelecrystalStackPrototype, ent);
        }

        private void OnWarDeclared(ref WarDeclaredEvent args)
        {
            var query = EntityQueryEnumerator<AssaultPodConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                if (comp.CostPayed)
                    continue;

                comp.WarDeclared = true;

                if (comp.InsertedTelecrystals != 0)
                    _chat.DispatchFilteredAnnouncement(
                        Filter.BroadcastMap(Transform(uid).MapID),
                        Loc.GetString(comp.WarDeclaredFailedDepartureAnnouncement),
                        sender: Loc.GetString(comp.NukieAnnouncementSender),
                        colorOverride: Color.DarkRed
                    );
                else
                    return;

                _stackSystem.SpawnNextToOrDrop(comp.InsertedTelecrystals, TelecrystalStackPrototype, uid);
                comp.InsertedTelecrystals = 0;
            }
        }

        private bool TryFindNukeOpsRule(out NukeopsRuleComponent? nukeopsRule)
        {
            foreach (var rule in _gameTicker.GetActiveGameRules<NukeopsRuleComponent>())
            {
                nukeopsRule = rule;
                return true;
            }

            nukeopsRule = null;
            return false;
        }
    }
}
