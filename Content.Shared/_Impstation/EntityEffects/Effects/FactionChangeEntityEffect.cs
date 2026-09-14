using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.EntityEffects.Effects;

///Ty to Mqole, Ada and Ruddy for the help.

public sealed partial class FactionChange : EntityEffectBase<FactionChange>
{
    [DataField(required: true)]
    public ProtoId<NpcFactionPrototype> Faction;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-faction-change",
            ("faction", Faction),
            ("chance", Probability));
}
