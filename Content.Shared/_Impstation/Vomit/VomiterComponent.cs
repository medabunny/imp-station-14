using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Medical;

/// <summary>
/// This component is used to denote which entities are able to vomit, and any unique properties their vomit may have.
/// </summary>
[RegisterComponent]
public sealed partial class VomiterComponent : Component
{
    [DataField]
    public float ChemMultiplier = 0.1f;

    [DataField]
    public ProtoId<SoundCollectionPrototype> VomitCollection = "Vomit";

    [DataField]
    public ProtoId<ReagentPrototype> VomitPrototype = "Vomit";
}
