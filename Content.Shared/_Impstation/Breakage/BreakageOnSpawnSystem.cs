using Content.Shared.Destructible;

namespace Content.Shared._Impstation.Breakage;

public sealed class BreakageOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly SharedDestructibleSystem _destructible = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BreakageOnSpawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BreakageOnSpawnComponent> ent, ref MapInitEvent args)
    {
        _destructible.BreakEntity(ent);
    }
}
