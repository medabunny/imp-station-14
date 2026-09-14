using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Destructible.Thresholds; // Imp

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Imp, changed description.
/// Spawns specified amount of entity at a random or empty tile on a station using TryGetRandomTile.
/// </summary>
[RegisterComponent, Access(typeof(RandomSpawnRule))]
public sealed partial class RandomSpawnRuleComponent : Component
{
    /// <summary>
    /// The entity to be spawned.
    /// </summary>
    [DataField("prototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = string.Empty;

    /// <summary>
    /// Imp.
    /// Spawn effect to be spawned on the same tile as the entity spawned. Does not follow the entity.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnEffect;

    /// <summary>
    /// Imp.
    /// Variation in the amount of entities to spawn.
    /// </summary>
    [DataField]
    public MinMax MinMaxEntities = new(1, 1);

    /// <summary>
    /// Imp.
    /// Whether to spawn entities on tiles without dynamic or static entities.
    /// </summary>
    [DataField]
    public bool EmptyTilesOnly;

    /// <summary>
    /// Imp.
    /// Announcement to be played when a station event with this rule is added.
    /// </summary>
    [DataField]
    public LocId? Announcement;
}
