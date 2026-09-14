using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Body;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] //imp edit, AutoGenerateComponentState added since ThermalVisibility uses AutoNetworkedField
[Access(typeof(BodySystem))]
public sealed partial class BodyComponent : Component
{
    public const string ContainerID = "body_organs";

    /// <summary>
    /// The actual container with entities with <see cref="OrganComponent" /> in it
    /// </summary>
    [ViewVariables]
    public Container? Organs;

    // EE edit
    // TODO IMP: should probably be its own component?
    /// <summary>
    /// Determines if the entity will show up on thermal scanners for thermal vision.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ThermalVisibility = true;
}

/// <summary>
/// Raised on organ entity, when it is inserted into a body
/// </summary>
[ByRefEvent]
public readonly record struct OrganGotInsertedEvent(EntityUid Target);

/// <summary>
/// Raised on organ entity, when it is removed from a body
/// </summary>
[ByRefEvent]
public readonly record struct OrganGotRemovedEvent(EntityUid Target);

/// <summary>
/// Raised on body entity, when an organ is inserted into it
/// </summary>
[ByRefEvent]
public readonly record struct OrganInsertedIntoEvent(EntityUid Organ);

/// <summary>
/// Raised on body entity, when an organ is removed from it
/// </summary>
[ByRefEvent]
public readonly record struct OrganRemovedFromEvent(EntityUid Organ);
