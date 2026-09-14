using Content.Shared._Goobstation.Changeling;
using Content.Shared.Body;
using Content.Shared.Gibbing;
using Content.Shared.Mind;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Changeling;

public sealed partial class GoobChangelingEggSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<GoobChangelingEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);

            Cycle(uid, comp);
        }
    }
    public void Cycle(EntityUid uid, GoobChangelingEggComponent comp)
    {
        if (comp.active == false)
        {
            comp.active = true;
            return;
        }

        var newUid = Spawn("MobMonkey", Transform(uid).Coordinates);

        EnsureComp<MindComponent>(newUid);
        _mind.TransferTo(comp.lingMind, newUid);

        EnsureComp<GoobChangelingComponent>(newUid);

        EntityManager.AddComponent(newUid, comp.lingStore);

        _gibbing.Gib(uid);
    }
}
