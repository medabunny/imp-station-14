using Content.Shared.EntityConditions;
using Content.Shared.NPC;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.EntityConditions.Conditions;

/// <summary>
/// Returns true if the entity has the ActiveNPC component.
/// </summary>
/// <inheritdoc cref="EntityConditionSystem{T, TCondition}"/>
public sealed partial class NPCEntityConditionSystem : EntityConditionSystem<ActiveNPCComponent, NPCEntityCondition>
{
    protected override void Condition(Entity<ActiveNPCComponent> entity, ref EntityConditionEvent<NPCEntityCondition> args)
    {
        args.Result = true;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class NPCEntityCondition : EntityConditionBase<NPCEntityCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
            Loc.GetString("entity-condition-guidebook-is-npc", ("isNPC", !Inverted));
}
