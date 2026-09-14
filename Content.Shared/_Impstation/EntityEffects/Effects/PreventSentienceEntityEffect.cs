using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.EntityEffects.Effects;

public sealed partial class PreventSentience : EntityEffectBase<PreventSentience>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    => Loc.GetString("reagent-effect-guidebook-prevent-sentience", ("chance", Probability));
}
