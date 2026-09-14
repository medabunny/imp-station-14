using Content.Shared.Objectives.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.Antag;

/// <summary>
/// Gives a player a set of objectives when taking control of a ghost role entity without relying on a gamerule.
/// </summary>
[RegisterComponent]
public sealed partial class GhostTakeoverEntityObjectivesComponent : Component
{
    /// <summary>
    /// The set of objective to be given to player taking over the entity.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId<ObjectiveComponent>> Objectives = new();
}
