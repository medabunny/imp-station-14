using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Used for the Unborgable trait. Prevents the entity's brain from being inserted into an MMI.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnborgableComponent : Component
{
    /// <summary>
    ///     Popup to display when the brain is deleted.
    /// </summary>
    [DataField]
    public LocId FailPopup = "borg-mmi-fail-popup";

    /// <summary>
    ///     Alternative popup to display if the brain is a diona nymph,
    ///     which fails but is not deleted.
    /// </summary>
    [DataField]
    public LocId NymphFailPopup = "borg-mmi-fail-popup-nymph";

    /// <summary>
    ///     Sound that plays when the brain is deleted.
    /// </summary>
    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
}
