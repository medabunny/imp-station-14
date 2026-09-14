using Content.Server.Stack;
using Content.Shared.Gibbing;
using Content.Shared.Storage.Components;
using Content.Shared.Whitelist;
using Content.Shared.Xenoarchaeology.Equipment;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Shared.Collections;
using Robust.Shared.Random;
using Content.Shared._Goobstation.Changeling; //imp
using Content.Server.Xenoarchaeology.Artifact;
using Content.Shared.Damage.Systems; //imp
using Content.Shared.Xenoarchaeology.Artifact.Components; //imp

namespace Content.Server.Xenoarchaeology.Equipment.Systems;

/// <inheritdoc/>
public sealed class ArtifactCrusherSystem : SharedArtifactCrusherSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly XenoArtifactSystem _artifact = default!; //imp
    [Dependency] private readonly DamageableSystem _damageable = default!; //imp

    // TODO: Move to shared once StackSystem spawning is in Shared and we have RandomPredicted
    public override void FinishCrushing(Entity<ArtifactCrusherComponent, EntityStorageComponent> ent)
    {
        var (_, crusher, storage) = ent;
        StopCrushing((ent, ent.Comp1), false);
        AudioSystem.PlayPvs(crusher.CrushingCompleteSound, ent);
        crusher.CrushingSoundEntity = null;
        Dirty(ent, ent.Comp1);

        var contents = new ValueList<EntityUid>(storage.Contents.ContainedEntities);
        var coords = Transform(ent).Coordinates;
        foreach (var contained in contents)
        {
            if (_whitelistSystem.IsWhitelistPass(crusher.CrushingWhitelist, contained))
            {
                var amount = _random.Next(crusher.MinFragments, crusher.MaxFragments);
                var stacks = _stack.SpawnMultipleAtPosition(crusher.FragmentStackProtoId, amount, coords);
                foreach (var stack in stacks)
                {
                    ContainerSystem.Insert((stack, null, null, null), crusher.OutputContainer);
                }
            }

            if (TryComp<XenoArtifactComponent>(contained, out var artifact)) //imp if statement and all code within
            {
                var unlocking = EnsureComp<XenoArtifactUnlockingComponent>(contained);
                _artifact.FinishUnlockingState((contained, unlocking, artifact));
            }

            if (!HasComp<GoobChangelingComponent>(contained)) //#IMP if statement to make changelings immune to gibbing
            {
                var gibs = _gibbing.Gib(contained);
                foreach (var gib in gibs)
                {
                    ContainerSystem.Insert((gib, null, null, null), crusher.OutputContainer);
                }
            }
            else //#IMP deals an extra burst of damage to changelings at the end
            {
                _damageable.TryChangeDamage(contained, crusher.CrushingDamage * crusher.NonGibbedDamageMult);
            }
        }
    }
}
