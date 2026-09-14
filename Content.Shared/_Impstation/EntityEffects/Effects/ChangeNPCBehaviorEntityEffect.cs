using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.EntityEffects.Effects;

public sealed partial class ChangeNPCBehavior : EntityEffectBase<ChangeNPCBehavior>
{
    [DataField(required: true)]
    public string rootTask = string.Empty;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    => Loc.GetString("reagent-effect-guidebook-change-npc-behavior", ("chance", Probability));
}
