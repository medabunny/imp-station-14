using System.Linq;
using System.Numerics;
using Content.Server.AlertLevel;
using Content.Server.Announcements.Systems;
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
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Impstation.Shuttles.Systems
{
    public sealed class AssaultPodConsoleSystem : EntitySystem
    {
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
        [Dependency] private readonly AnnouncerSystem _announcer = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly LockSystem _lockSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly MapSystem _mapSystem = default!;
        [Dependency] private readonly NavMapSystem _navMap = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ShuttleSystem _shuttle = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly StationSystem _station = default!;
        private static readonly ProtoId<StackPrototype> TelecrystalStackPrototype = "Telecrystal";
        private static readonly string CommandAnnouncementId = "commandReport";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AssaultPodConsoleComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<AssaultPodConsoleComponent, WarDeclaredEvent>(OnWarDeclared);
            SubscribeLocalEvent<AssaultPodConsoleComponent, DestructionEventArgs>(OnDestruction);
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
                    destroyFloor: true,
                    arrivalKnockRadius: comp.ArrivalKnockRadius,
                    travelSound: comp.TravelSound,
                    globalArrivalSound: comp.ArrivalSound);

                if (!TryFindNukeOpsRule(out var nukeopsRule)
                    || nukeopsRule?.TargetStation is not { } targetStation)
                    continue;

                var stationGrid = _station.GetLargestGrid((targetStation, null));
                if (stationGrid == null)
                    continue;

                var audio = _audio.PlayPvs(comp.TravelSound, stationGrid.Value);
                _audio.SetMapAudio(audio);

                var mapID = Transform(stationGrid.Value).MapID;
                var beacon = _navMap.GetNearestBeaconString(new MapCoordinates(comp.TravelCoordinates.Position, mapID));
                _alertLevelSystem.SetLevel(targetStation, comp.AlertLevel, true, true, true);
                _announcer.SendAnnouncement(
                    _announcer.GetAnnouncementId(CommandAnnouncementId),
                    Filter.BroadcastMap(mapID),
                    Loc.GetString(comp.DepartureStationAnnouncement, ("beacon", beacon)),
                    Loc.GetString(comp.StationAnnouncementSender),
                    Color.Cyan,
                    targetStation
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

            var mapUid = _mapSystem.GetMapOrInvalid(args.Coordinates.MapId);
            var gridCoords = _mapSystem.MapToGrid(mapUid, args.Coordinates);
            if (!TryComp<MapGridComponent>(gridCoords.EntityId, out var grid))
                return;

            var localCoords = gridCoords.Position;
            var tileRefs = _mapSystem.GetLocalTilesIntersecting(
                gridCoords.EntityId,
                grid,
                new Box2(localCoords + new Vector2(-ent.Comp.LandingVariationRange, -ent.Comp.LandingVariationRange),
                    localCoords + new Vector2(ent.Comp.LandingVariationRange, ent.Comp.LandingVariationRange))
                ).ToList();

            if (tileRefs.Count == 0)
                return;

            var chosenTile = _random.Pick(tileRefs);
            ent.Comp.TravelCoordinates = _mapSystem.ToCoordinates(chosenTile, grid);
            ent.Comp.LaunchTime = _timing.CurTime + ent.Comp.TimeTillLaunch;

            var beacon = _navMap.GetNearestBeaconString(args.Coordinates, true);
            _chat.DispatchFilteredAnnouncement(
                Filter.BroadcastMap(Transform(ent).MapID),
                Loc.GetString(ent.Comp.BeginDepartureAnnouncement, ("beacon", beacon)),
                sender: Loc.GetString(ent.Comp.NukieAnnouncementSender),
                announcementSound: ent.Comp.DepartureAnnouncementSound,
                colorOverride: Color.DarkRed
            );
        }

        private void OnWarDeclared(Entity<AssaultPodConsoleComponent> ent, ref WarDeclaredEvent args)
        {
            ent.Comp.WarDeclared = true;

            if (ent.Comp.InsertedTelecrystals != 0)
                _chat.DispatchFilteredAnnouncement(
                    Filter.BroadcastMap(Transform(ent).MapID),
                    Loc.GetString(ent.Comp.WarDeclaredFailedDepartureAnnouncement),
                    sender: Loc.GetString(ent.Comp.NukieAnnouncementSender),
                    colorOverride: Color.DarkRed
                );
            else
                return;

            _stackSystem.SpawnNextToOrDrop(ent.Comp.InsertedTelecrystals, TelecrystalStackPrototype, ent);
            ent.Comp.InsertedTelecrystals = 0;
        }

        private void OnDestruction(Entity<AssaultPodConsoleComponent> ent, ref DestructionEventArgs args)
        {
            if (ent.Comp.Launched)
                return;

            _stackSystem.SpawnNextToOrDrop(ent.Comp.InsertedTelecrystals, TelecrystalStackPrototype, ent);
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
