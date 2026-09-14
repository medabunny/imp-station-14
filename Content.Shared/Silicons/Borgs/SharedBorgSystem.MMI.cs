using Content.Shared.Mind.Components;
using Content.Shared.Roles.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Chemistry.Components; // imp unborgable
using Content.Shared.Chemistry.EntitySystems; // imp unborgable
using Content.Shared.Containers.ItemSlots; // imp unborgable
using Content.Shared.Fluids; // imp unborgable
using Content.Shared.Popups; // imp unborgable
using Content.Shared.Tag; // imp unborgable
using Content.Shared.Traits.Assorted; // imp unborgable
using Robust.Shared.Audio.Systems; // imp unborgable
using Robust.Shared.Enums; // imp; for Gender
using Robust.Shared.GameObjects.Components.Localization; // imp; for Grammar

namespace Content.Shared.Silicons.Borgs;

public abstract partial class SharedBorgSystem
{
    [Dependency] private readonly GrammarSystem _grammar = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!; // imp
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!; // imp
    [Dependency] private readonly TagSystem _tag = default!; // imp for unborgable

    private static readonly EntProtoId SiliconBrainRole = "MindRoleSiliconBrain";
    private static readonly ProtoId<TagPrototype> DionaNymph = "DionaNymph";

    public void InitializeMMI()
    {
        SubscribeLocalEvent<MMIComponent, ComponentInit>(OnMMIInit);
        SubscribeLocalEvent<MMIComponent, EntInsertedIntoContainerMessage>(OnMMIEntityInserted);
        SubscribeLocalEvent<MMIComponent, MindAddedMessage>(OnMMIMindAdded);
        SubscribeLocalEvent<MMIComponent, MindRemovedMessage>(OnMMIMindRemoved);

        SubscribeLocalEvent<MMIComponent, EntRemovedFromContainerMessage>(OnMMILinkedRemoved);

        SubscribeLocalEvent<MMIComponent, ItemSlotInsertAttemptEvent>(OnMMIAttemptInsert); // imp
    }

    private void OnMMIInit(Entity<MMIComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent.Owner, ent.Comp.BrainSlotId, ent.Comp.BrainSlot);
    }

    private void OnMMIEntityInserted(Entity<MMIComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState)
            return; // The changes are already networked with the same game state

        if (args.Container.ID != ent.Comp.BrainSlotId)
            return;

        var brain = args.Entity;

        if (HasComp<UnborgableComponent>(brain)) // imp add
            return;


        //IMP EDIT: keep the pronouns of the brain inserted
        var grammar = EnsureComp<GrammarComponent>(ent);
        if (TryComp<GrammarComponent>(brain, out var formerSelf))
        {
            _grammar.SetGender((ent, grammar), formerSelf.Gender);
            //man-machine interface is not a proper noun, so i'm not setting proper here
        }
        //END IMP EDIT

        if (_mind.TryGetMind(brain, out var mindId, out var mindComp))
        {
            _mind.TransferTo(mindId, ent.Owner, true, mind: mindComp);

            if (!_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                _roles.MindAddRole(mindId, SiliconBrainRole, silent: true);
        }

        _appearance.SetData(ent.Owner, MMIVisuals.BrainPresent, true);
    }

    private void OnMMIMindAdded(Entity<MMIComponent> ent, ref MindAddedMessage args)
    {
        _appearance.SetData(ent.Owner, MMIVisuals.HasMind, true);
    }

    private void OnMMIMindRemoved(Entity<MMIComponent> ent, ref MindRemovedMessage args)
    {
        //IMP EDIT: no brain, no gender, bucko
        if (TryComp<GrammarComponent>(ent, out var grammar))
            _grammar.SetGender((ent, grammar), Gender.Neuter); // it/its
        //END IMP EDIT
        _appearance.SetData(ent.Owner, MMIVisuals.HasMind, false);
    }

    private void OnMMILinkedRemoved(Entity<MMIComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState)
            return; // The changes are already networked with the same game state

        if (args.Container.ID != ent.Comp.BrainSlotId)
            return;

        if (_mind.TryGetMind(ent, out var mindId, out var mindComp))
        {
            _mind.TransferTo(mindId, args.Entity, true, mind: mindComp);

            if (_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                _roles.MindRemoveRole<SiliconBrainRoleComponent>(mindId);
        }

        _appearance.SetData(ent, MMIVisuals.BrainPresent, false);
    }

    // imp add
    private void OnMMIAttemptInsert(Entity<MMIComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        var brain = args.Item;
        if (!TryComp<UnborgableComponent>(brain, out var unborgable))
            return;

        // IMP EDIT: different behavior if brain is a diona nymph
        if (_tag.HasTag(brain, DionaNymph))
        {
            args.Cancelled = true; // to ensure ejection happens within method
            _popup.PopupPredicted(Loc.GetString(unborgable.NymphFailPopup), ent, ent, PopupType.MediumCaution); // display different failure popup
            _itemSlots.TryEjectToHands(brain, args.Slot, args.User); // eject nymph back out of OSI
            return; // exit OnMMIAttemptInsert WITHOUT dissolving & killing the nymph
        }
        // END IMP EDIT

        _popup.PopupPredicted(Loc.GetString(unborgable.FailPopup), ent, ent, PopupType.MediumCaution);
        _audio.PlayPredicted(unborgable.FailSound, ent, ent);

        if (_solution.TryGetSolution(brain, "food", out var solution))
        {
            if (solution != null)
            {
                var solutions = (Entity<SolutionComponent>)solution;
                _puddle.TrySpillAt(Transform(ent).Coordinates, solutions.Comp.Solution, out _);
            }
        }
        EntityManager.PredictedQueueDeleteEntity(brain);
    }
}
