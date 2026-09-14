using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;

namespace Content.Server._Impstation.Antag;

/// <summary>
///  Adds objectives to a player mind on taking control of a ghost role entity.
/// </summary>
public sealed partial class GhostTakeoverEntityObjectivesSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostTakeoverEntityObjectivesComponent, TakeGhostRoleEvent>(GrantObjectives);
    }

    /// <summary>
    /// Grant the player objectives upon taking control
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="args"></param>
    private void GrantObjectives(Entity<GhostTakeoverEntityObjectivesComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (!_mind.TryGetMind(args.Player, out var mindId, out var mind))
            return;
        foreach (var objective in ent.Comp.Objectives)
        {
            _mind.TryAddObjective(mindId, mind, objective);
        }

    }
}
