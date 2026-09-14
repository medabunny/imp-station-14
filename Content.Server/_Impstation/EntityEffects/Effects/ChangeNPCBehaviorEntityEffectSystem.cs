using Content.Server.NPC.HTN;
using Content.Shared.NPC.Components;
using Content.Shared.EntityEffects;
using Content.Shared._Impstation.EntityEffects.Effects;

namespace Content.Server._Impstation.EntityEffects.Effects;

/// <summary>
/// If target has HTNComponent, changes their root task, removes FactionExceptionComponent, and resets their targeting.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class ChangeNPCBehaviorEntityEffectSystem : EntityEffectSystem<HTNComponent, ChangeNPCBehavior>
{
    protected override void Effect(Entity<HTNComponent> entity, ref EntityEffectEvent<ChangeNPCBehavior> args)
    {
        // Changes the targets root task to whatever htnCompound is set.
        var htn = entity.Comp;
        htn.RootTask = new HTNCompoundTask { Task = args.Effect.rootTask };

        // Removes FactionExceptionComponent
        RemComp<FactionExceptionComponent>(entity);

        // Resets their targeting.
        htn.Blackboard.Remove<EntityUid>("Target");
    }
}
