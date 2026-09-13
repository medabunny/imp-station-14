using Content.Shared.Destructible.Thresholds;
using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.StatusEffectNew.Components;

/// <summary>
/// A status effect meant to replicate lumbago, aka lower back pain.
/// Occasionally send popups about back pain, makes pulling slower, and occasionally causes a blanket move speed debuff.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LumbagoStatusEffectComponent : Component
{
    /// <summary>
    /// Time to the next reminder.
    /// </summary>
    [AutoNetworkedField]
    public TimeSpan LumbagoReminderDelay;

    /// <summary>
    /// The minimum and maximum delay between reminders in seconds
    /// </summary>
    [DataField]
    public MinMax LumbagoReminderDelayMinMax = new (30,600);

    /// <summary>
    /// Time to the next flare up.
    /// </summary>
    [AutoNetworkedField]
    public TimeSpan LumbagFlareUpDelay;

    /// <summary>
    /// The minimum and maximum delay between flareups in seconds
    /// </summary>
    [DataField]
    public MinMax LumbagoFlareUpDelayMinMax = new (300,1800);

    /// <summary>
    /// The effects pulling walk speed modifier
    /// </summary>
    [DataField]
    public float PullWalkSpeedMod = 0.5f;

    /// <summary>
    /// The effects sprinting walk speed modifier
    /// </summary>
    [DataField]
    public float PullSprintSpeedMod = 0.5f;

    /// <summary>
    /// The minimum duration of a flare up.
    /// </summary>
    [DataField]
    public MinMax FlareUpDurationMinMax = new (10,60);

    /// <summary>
    /// The blanket move speed modifier of a flare up.
    /// </summary>
    [DataField]
    public float FlareUpMovementSpeedMod = 0.5f;

    /// <summary>
    /// The set of reminders.
    /// </summary>
    [DataField]
    public List<LocId> MildPainReminders = new();

    /// <summary>
    /// The set of reminders during a flair up.
    /// </summary>
    [DataField]
    public List<LocId> BadPainReminders = new();

    /// <summary>
    /// The target entity of the status effect.
    /// </summary>
    public EntityUid Affected;
}
