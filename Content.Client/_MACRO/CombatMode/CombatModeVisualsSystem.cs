using Content.Client.DamageState;
using Content.Shared._MACRO.CombatMode;
using Content.Shared.CombatMode;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._MACRO.CombatMode;

public sealed partial class CombatModeVisualsSystem : SharedCombatModeVisualsSystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CombatModeVisualsComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnStartup(Entity<CombatModeVisualsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((ent, sprite), CombatModeVisualLayers.Combat, out var _, false))
            return;

        var layer = _sprite.AddLayer((ent, sprite), ent.Comp.Sprite);
        _sprite.LayerMapSet((ent, sprite), CombatModeVisualLayers.Combat, layer);
    }

    private void OnAppearanceChanged(Entity<CombatModeVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (!TryComp<CombatModeComponent>(ent, out var combat))
            return;

        // make sure we can sync the frames
        if (!_sprite.TryGetLayer((ent, args.Sprite), CombatModeVisualLayers.Combat, out var combatLayer, true))
            return;

        // turn on combat visuals if the mob is alive and in combat mode. otherwise turn them off
        _sprite.LayerSetVisible(combatLayer, _mobState.IsAlive(ent) && combat.IsInCombatMode);

        if (!_sprite.TryGetLayer((ent, args.Sprite), DamageStateVisualLayers.Base, out var baseLayer, false))
            return;

        // handle hiding/unhiding the base layer if applicable
        if (ent.Comp.HideBaseLayer && _mobState.IsAlive(ent))
            _sprite.LayerSetVisible(baseLayer, !combat.IsInCombatMode);
        else if (ent.Comp.HideBaseLayer)
            _sprite.LayerSetVisible(baseLayer, true);

        // then sync them to the base animation
        if (combatLayer.AutoAnimated)
            _sprite.LayerSetAnimationTime(combatLayer, baseLayer.AnimationTime);
    }
}
