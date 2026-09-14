using System.Linq;
using System.Numerics;
using Content.Client.Stealth;
using Content.Shared.Body;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared.Heretic.Components;
using Robust.Shared.Prototypes;


namespace Content.Client._EE.Overlays.Switchable;

public sealed class SerpentFocusOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;


    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly StealthSystem _stealth;
    private readonly ContainerSystem _container;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<SerpentFocusRenderEntry> _entries = [];

    public SerpentFocusComponent? Comp;

    private static readonly ProtoId<ShaderPrototype> CircleShader = "CircleMask";
    private readonly ShaderInstance _circleMaskShader;

    public SerpentFocusOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _stealth = _entity.System<StealthSystem>();
        _sprite = _entity.System<SpriteSystem>();
        _circleMaskShader = _prototypeManager.Index(CircleShader).InstanceUnique();

        ZIndex = -1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null || Comp is null)
            return;

        var worldHandle = args.WorldHandle;
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var player = _player.LocalEntity;

        if (!_entity.TryGetComponent(player, out TransformComponent? playerXform))
            return;

        var accumulator = Math.Clamp(Comp.PulseAccumulator, 0f, Comp.PulseTime);
        var alpha = Comp.PulseTime <= 0f ? 1f : float.Lerp(1f, 0f, accumulator / Comp.PulseTime);

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;

        _entries.Clear();
        var entities = _entity.EntityQueryEnumerator<BodyComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out var body, out var sprite, out var xform))
        {
            if (!CanSee(uid, sprite))
                continue;

            var entity = uid;

            if (_container.TryGetOuterContainer(uid, xform, out var container))
            {
                var owner = container.Owner;

                if (_entity.TryGetComponent<SpriteComponent>(owner, out var ownerSprite)
                    && _entity.TryGetComponent<TransformComponent>(owner, out var ownerXform))
                {
                    entity = owner;
                    sprite = ownerSprite;
                    xform = ownerXform;
                }
            }

            if (_entries.Any(e => e.Ent.Owner == entity))
                continue;

            _entries.Add(new SerpentFocusRenderEntry((entity, sprite, xform), mapId, eyeRot));
        }

        var viewport = args.WorldBounds;
        //draw a black rectangle over the view
        worldHandle.DrawRect(viewport, Color.Black);
        //draw living things over that
        foreach (var entry in _entries)
        {
            Render(entry.Ent, entry.Map, worldHandle, entry.EyeRot, Comp.Color, alpha);
        }
        //cleanup
        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);
    }

    private void Render(Entity<SpriteComponent, TransformComponent> ent,
        MapId? map,
        DrawingHandleWorld handle,
        Angle eyeRot,
        Color color,
        float alpha)
    {
        var (uid, sprite, xform) = ent;
        if (xform.MapID != map || !CanSee(uid, sprite))
            return;

        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        var originalColor = sprite.Color;

        _sprite.SetColor((ent, sprite), color.WithAlpha(alpha));
        _sprite.RenderSprite(ent, handle, eyeRot, rotation, position);
        _sprite.SetColor((ent, sprite), originalColor);
    }

    private bool CanSee(EntityUid uid, SpriteComponent sprite)
    {
        return sprite.Visible && (!_entity.TryGetComponent(uid, out StealthComponent? stealth) ||
                                  _stealth.GetVisibility(uid, stealth) > 0.5f);
    }
}

public record struct SerpentFocusRenderEntry(
    Entity<SpriteComponent, TransformComponent> Ent,
    MapId? Map,
    Angle EyeRot);
