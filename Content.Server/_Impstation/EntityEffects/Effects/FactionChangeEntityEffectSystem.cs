using Content.Shared.EntityEffects;
using Content.Shared._Impstation.EntityEffects.Effects;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;

namespace Content.Server._Impstation.EntityEffects.Effects;

/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class FactionChangeEntityEffectSystem : EntityEffectSystem<MetaDataComponent, FactionChange>
{
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<FactionChange> args)
    {
        //do nothing if the faction has no faction member comp
        if (!TryComp<NpcFactionMemberComponent>(entity, out var npcFactionMember))
            return;

        //make it a tuple so we don't have to re-tuple it twice for the factionSystem calls
        var entAsTuple = (entity, npcFactionMember);

        //do the factionSystem stuff
        _faction.ClearFactions(entAsTuple);
        _faction.AddFaction(entAsTuple, args.Effect.Faction);
    }

}
