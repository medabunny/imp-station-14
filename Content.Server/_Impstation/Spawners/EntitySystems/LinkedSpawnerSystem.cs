using Content.Server._Impstation.Spawners.Components;
using Content.Shared.Coordinates;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Impstation.Spawners.EntitySystems;

/// <summary>
/// A spawner that will spawn an entity once at a random spawner.
/// </summary>
public sealed class LinkedSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private Dictionary<EntProtoId, List<Entity<LinkedSpawnerComponent>>> _cache = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LinkedSpawnerComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LinkedSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    // add itself to the cache, with the prototype it spawns as the key
    private void OnComponentInit(Entity<LinkedSpawnerComponent> ent, ref ComponentInit args)
    {
        if (!_cache.ContainsKey(ent.Comp.Prototype))
            _cache.Add(ent.Comp.Prototype, []);

        _cache[ent.Comp.Prototype].Add(ent);
    }

    // go through every key in the cache and spawn its prototype at a random spawner, then delete the spawners and the key from the cache.
    private void OnMapInit(Entity<LinkedSpawnerComponent> ent, ref MapInitEvent args)
    {
        foreach (var prototype in _cache.Keys)
        {
            // make a dictionary to store each spawner and a weight
            var randomDict = new Dictionary<Entity<LinkedSpawnerComponent>, float>();

            foreach (var possibleSpawner in _cache[prototype])
            {
                randomDict.Add(possibleSpawner, possibleSpawner.Comp.Weight);
            }

            // pick a random spawner from the dictionary based on weights
            var randomSpawner = _random.Pick(randomDict);

            // spawn the entity at the chosen spawner
            SpawnAtPosition(randomSpawner.Comp.Prototype, randomSpawner.Owner.ToCoordinates());

            // remove the prototype key from the cache
            _cache.Remove(prototype);
        }
    }
}
