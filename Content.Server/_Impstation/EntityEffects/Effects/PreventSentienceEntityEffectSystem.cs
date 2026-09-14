using Content.Server.Ghost.Roles.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.EntityEffects;
using Content.Shared._Impstation.EntityEffects.Effects;

namespace Content.Server._Impstation.EntityEffects.Effects;

/// <summary>
/// Removes ghost role and random sentience chance from target.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class PreventSentienceEntityEffectSystem : EntityEffectSystem<MetaDataComponent, PreventSentience>
{
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<PreventSentience> args)
    {
        RemComp<GhostRoleComponent>(entity);
        RemComp<SentienceTargetComponent>(entity);
    }
}
