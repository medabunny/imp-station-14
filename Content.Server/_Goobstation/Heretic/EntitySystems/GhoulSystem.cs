using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Humanoid;
using Content.Shared.Administration.Systems;
using Content.Shared.Body;
using Content.Shared.Examine;
using Content.Shared.Gibbing;
using Content.Shared._Impstation.Heretic; // imp edit
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Shared.Nutrition.Components;
using Content.Shared.Temperature.Components;

namespace Content.Server.Heretic.EntitySystems;

public sealed class GhoulSystem : Shared.Heretic.EntitySystems.SharedGhoulSystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;

    public void GhoulifyEntity(Entity<GhoulComponent> ent)
    {
        RemComp<RespiratorComponent>(ent);
        RemComp<BarotraumaComponent>(ent);
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        RemComp<ReproductiveComponent>(ent);
        RemComp<ReproductivePartnerComponent>(ent);
        RemComp<TemperatureComponent>(ent);

        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            // make them "have no eyes" and grey
            // this is clearly a reference to grey tide
            var greycolor = Color.FromHex("#505050");
            _humanoid.SetSkinColor(ent, greycolor, true, false, humanoid);
            _humanoid.SetBaseLayerColor(ent, HumanoidVisualLayers.Eyes, greycolor, true, humanoid);
        }

        _rejuvenate.PerformRejuvenate(ent);

        if (!TryComp<MobThresholdsComponent>(ent, out var th))
            return;

        // imp edit start
        _threshold.TryGetDeadThreshold(ent, out var deadThr);
        if (deadThr == null)
        // fallback for if an entity has no dead threshold. Somehow.
        {
            _threshold.SetMobStateThreshold(ent, ent.Comp.FallbackHealth, MobState.Dead, th);
            _threshold.SetMobStateThreshold(ent, ent.Comp.FallbackHealth / 1.25f, MobState.Critical, th);
        }
        else
        {
            if (deadThr > ent.Comp.MaxHealth)
                deadThr = ent.Comp.MaxHealth;
            _threshold.SetMobStateThreshold(ent, (Shared.FixedPoint.FixedPoint2)(deadThr / ent.Comp.HealthDivisor), MobState.Dead, th);
            _threshold.SetMobStateThreshold(ent, (Shared.FixedPoint.FixedPoint2)(deadThr / ent.Comp.HealthDivisor) / 1.25f, MobState.Critical, th);
        }
        // imp edit end
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhoulComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<GhoulComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<GhoulComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnInit(Entity<GhoulComponent> ent, ref ComponentInit args)
    {
        GhoulifyEntity(ent);
    }
    private void OnExamine(Entity<GhoulComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"[color=red]{Loc.GetString("heretic-ghoul-examine", ("ent", args.Examined))}[/color]");
    }

    private void OnMobStateChange(Entity<GhoulComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            _gibbing.Gib(ent);
    }
}
