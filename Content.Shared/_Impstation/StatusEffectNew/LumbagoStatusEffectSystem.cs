using Content.Shared._Impstation.StatusEffectNew.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Impstation.StatusEffectNew;
/// <summary>
/// System for the lumbago status effect.
/// Occasionally send popups about back pain, makes pulling slower, and occasionally causes a blanket move speed debuff.
/// </summary>
public sealed class LumbagoStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private readonly TimeSpan _lumbagoUpdateInterval= TimeSpan.FromSeconds(1);
    private TimeSpan _lumbagoUpdateTimer = TimeSpan.Zero;
    private readonly EntProtoId _flareUpStatusEffect = "LumbagoFlareUpSlowdownStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LumbagoStatusEffectComponent, StatusEffectAppliedEvent>(StatusEffectApplied);
        SubscribeLocalEvent<LumbagoStatusEffectComponent, StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent>>(TryModifyMovementSpeed);
    }


    private void StatusEffectApplied(Entity<LumbagoStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        ent.Comp.Affected = args.Target;
        //TODO: Replace with random predicted when we get that.
        var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, args.Target.GetHashCode());
        var rand = new System.Random(seed);
        ent.Comp.LumbagFlareUpDelay=_timing.CurTime + // we're setting the timers here so doing it like this is okay
                                    TimeSpan.FromSeconds(rand.NextInt64(ent.Comp.LumbagoFlareUpDelayMinMax.Min, ent.Comp.LumbagoFlareUpDelayMinMax.Max));
        ent.Comp.LumbagoReminderDelay=_timing.CurTime +
                                      TimeSpan.FromSeconds(ent.Comp.LumbagoReminderDelayMinMax.Min,ent.Comp.LumbagoReminderDelayMinMax.Max);
    }

    /// <summary>
    /// Selectively modifies pulling movespeed.
    /// </summary>
    private void TryModifyMovementSpeed(Entity<LumbagoStatusEffectComponent> ent, ref StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
       if (!HasComp<ActivePullerComponent>(ent.Comp.Affected))
            return;
       args.Args.ModifySpeed(ent.Comp.PullWalkSpeedMod, ent.Comp.PullSprintSpeedMod);
    }

    /// <summary>
    /// Every second we check the flare up timers of each entity with the status effect.
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if(_timing.CurTime < _lumbagoUpdateTimer)
            return;

        var query = EntityQueryEnumerator<LumbagoStatusEffectComponent,StatusEffectComponent>();
        _lumbagoUpdateTimer += _lumbagoUpdateInterval;

        while (query.MoveNext(out _, out var lumbagoComp, out var statusComp))
        {
            if (statusComp.AppliedTo is not { } statusOwner)
                continue;

            //TODO: Replace with random predicted when we get that.
            var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, statusOwner.GetHashCode());
            var rand = new System.Random(seed);

            //if we have reached the time for the next flare up, trigger it.
            if (_timing.CurTime >= lumbagoComp.LumbagFlareUpDelay)
            {
                var duration = TimeSpan.FromSeconds(rand.NextFloat(lumbagoComp.FlareUpDurationMinMax.Min, lumbagoComp.FlareUpDurationMinMax.Max));
                _movementMod.TryAddMovementSpeedModDuration(statusOwner, _flareUpStatusEffect, duration,lumbagoComp.FlareUpMovementSpeedMod);
                lumbagoComp.LumbagFlareUpDelay += duration + TimeSpan.FromSeconds(rand.NextInt64(lumbagoComp.LumbagoFlareUpDelayMinMax.Min, lumbagoComp.LumbagoFlareUpDelayMinMax.Max));
                DirtyEntity(statusOwner);
            }

            //Send a reminder to the player when the timer runs out and there is a flair up occuring.
            if (_timing.CurTime >= lumbagoComp.LumbagoReminderDelay && _statusEffects.HasStatusEffect(statusOwner, _flareUpStatusEffect))
            {
                var selected = rand.Next(lumbagoComp.BadPainReminders.Count);
                if(!lumbagoComp.BadPainReminders.TryGetValue(selected, out var reminder))
                    return;

                _popup.PopupClient(Loc.GetString(reminder), statusOwner, statusOwner, PopupType.SmallCaution);

                lumbagoComp.LumbagoReminderDelay += TimeSpan.FromSeconds(lumbagoComp.LumbagoReminderDelayMinMax.Min,lumbagoComp.LumbagoReminderDelayMinMax.Max);
                DirtyEntity(statusOwner);

            }
            //Send a reminder to the player when the timer runs out.
            else if (_timing.CurTime >= lumbagoComp.LumbagoReminderDelay)
            {
                var selected = rand.Next(lumbagoComp.MildPainReminders.Count);
                if(!lumbagoComp.MildPainReminders.TryGetValue(selected, out var reminder))
                    return;

                _popup.PopupClient(Loc.GetString(reminder), statusOwner, statusOwner);

                lumbagoComp.LumbagoReminderDelay += TimeSpan.FromSeconds(lumbagoComp.LumbagoReminderDelayMinMax.Min,lumbagoComp.LumbagoReminderDelayMinMax.Max);
                DirtyEntity(statusOwner);
            }

        }

    }
}
