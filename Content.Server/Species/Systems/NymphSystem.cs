using Content.Server.Mind;
using Content.Server.Zombies;
using Content.Shared.Body;
using Content.Shared.Species.Components;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Components; // imp unborgable
using Content.Shared.Tag; // imp unborgable
using Content.Shared.Traits.Assorted; // imp unborgable

namespace Content.Server.Species.Systems;

public sealed partial class NymphSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly BodySystem _body = default!; // imp edit for Unborgable

    private static readonly ProtoId<TagPrototype> Brain = "Brain"; // imp edit for Unborgable

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NymphComponent, OrganGotRemovedEvent>(OnRemovedFromPart);

        SubscribeLocalEvent<BodyComponent, UnborgableDionaRelayNymphEvent>(_body.RelayEvent); // imp edit for Unborgable
        SubscribeLocalEvent<BrainComponent, BodyRelayedEvent<UnborgableDionaRelayNymphEvent>>(OnRelayUnborgableNymph); // imp edit for Unborgable
    }

    private void OnRemovedFromPart(EntityUid uid, NymphComponent comp, ref OrganGotRemovedEvent args)
    {
        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Target))
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(comp.EntityPrototype, out var entityProto))
            return;

        // Get the organs' position & spawn a nymph there
        var coords = Transform(uid).Coordinates;
        var nymph = SpawnAtPosition(entityProto.ID, coords);

        if (HasComp<ZombieComponent>(args.Target)) // Zombify the new nymph if old one is a zombie
            _zombie.ZombifyEntity(nymph);

        // IMP EDIT START - Unborgable trait support
        if (HasComp<UnborgableComponent>(uid))
        {
            AddComp<UnborgableComponent>(nymph); // Add UnborgableComponent to brain nymph
            // Add UnborgableComponent to brain organ inside the nymph (dropped upon gibbing)

            var ev = new UnborgableDionaRelayNymphEvent();
            RaiseLocalEvent(nymph, ref ev);
        }
        // IMP EDIT END - Unborgable trait support

        // Move the mind if there is one and it's supposed to be transferred
        if (comp.TransferMind && _mindSystem.TryGetMind(uid, out var mindId, out var mind)) // imp early merge
            _mindSystem.TransferTo(mindId, nymph, mind: mind);

        // Delete the old organ
        QueueDel(uid);
    }

    // IMP EDIT START - Unborgable trait support
    private void OnRelayUnborgableNymph(Entity<BrainComponent> ent, ref BodyRelayedEvent<UnborgableDionaRelayNymphEvent> args)
    {
        AddComp<UnborgableComponent>(ent);
    }

    /// <summary>
    /// Raised to relay unborgable component to internal nymphs' brain.
    /// </summary>
    [ByRefEvent]
    public record struct UnborgableDionaRelayNymphEvent;
    // IMP EDIT END - Unborgable trait support
}
