using Content.Shared.Shuttles.Systems;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared.Shuttles.Components;

/// <summary>
/// Added to a component when it is queued or is travelling via FTL.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLComponent : Component
{
    // TODO Full game save / add datafields

    [ViewVariables]
    public FTLState State = FTLState.Available;

    [ViewVariables(VVAccess.ReadWrite)]
    public StartEndTime StateTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public float StartupTime = 0f;

    // Because of sphagetti, actual travel time is Math.Max(TravelTime, DefaultArrivalTime)
    [ViewVariables(VVAccess.ReadWrite)]
    public float TravelTime = 0f;

    [DataField]
    public EntProtoId? VisualizerProto = "FtlVisualizerEntity";

    [DataField, AutoNetworkedField]
    public EntityUid? VisualizerEntity;

    /// <summary>
    /// Coordinates to arrive it: May be relative to another grid (for docking) or map coordinates.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityCoordinates TargetCoordinates;

    [DataField, AutoNetworkedField]
    public Angle TargetAngle;

    /// <summary>
    /// If we're docking after FTL what is the prioritised dock tag (if applicable).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public ProtoId<TagPrototype>? PriorityTag;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundTravel")]
    public SoundSpecifier? TravelSound = new SoundPathSpecifier("/Audio/Effects/Shuttle/hyperspace_progress.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f).WithLoop(true)
    };

    [DataField]
    public EntityUid? StartupStream;

    [DataField]
    public EntityUid? TravelStream;

    /// <summary>
    /// Imp.
    /// If a specific global arrival sound should play when you arrive instead of normal FTL arrival sound.
    /// Implement in the other public FTL methods if you want to modify from a non-position method FTL.
    /// </summary>
    [ViewVariables]
    public SoundSpecifier? GlobalArrivalSound;

    /// <summary>
    /// Imp.
    /// If to play the travel sound globally instead of only on the grid.
    /// Implement in the other public FTL methods if you want to modify from a non-position method FTL.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool GlobalTravelSound;

    /// <summary>
    /// Imp.
    /// If to destroy the floor area the FTL transports to
    /// Implement in the other public FTL methods if you want to modify from a non-position method FTL.
    /// </summary>
    [ViewVariables]
    public bool DestroyFloor;

    /// <summary>
    /// Imp.
    /// Radius of entities to knockdown on arrival, if null knockdown only on the shuttle
    /// Implement in the other public FTL methods if you want to modify from a non-position method FTL.
    /// </summary>
    [ViewVariables]
    public float ArrivalKnockdownRadius;
}
