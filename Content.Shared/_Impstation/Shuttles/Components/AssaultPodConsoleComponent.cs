using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Impstation.Shuttles.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AssaultPodConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public int InsertedTelecrystals;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? LaunchTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates TravelCoordinates;

    [ViewVariables]
    public bool CostPayed;

    [ViewVariables]
    public bool Launched;

    [ViewVariables]
    public bool WarDeclared;

    [DataField]
    public int Cost = 40;

    [DataField]
    public int LandingVariationRange = 5;

    [DataField]
    public float TravelTime = 11f;

    [DataField]
    public float ArrivalKnockRadius = 15;

    [DataField]
    public TimeSpan TimeTillLaunch = TimeSpan.FromSeconds(30);

    [DataField]
    public string AlertLevel = "red";

    [DataField]
    public LocId LockExamineText = "assault-pod-lock-examine";
    [DataField]
    public LocId BeginDepartureAnnouncement = "assault-pod-announcement-begin-departure";
    [DataField]
    public LocId WarDeclaredFailedDepartureAnnouncement = "assault-pod-announcement-war-declared-failed-departure";
    [DataField]
    public LocId DepartureStationAnnouncement = "station-announcement-departure";
    [DataField]
    public LocId NukieAnnouncementSender = "assault-pod-announcement-sender";
    [DataField]
    public LocId StationAnnouncementSender = "station-announcement-sender";
    [DataField]
    public SoundSpecifier DepartureAnnouncementSound = new SoundPathSpecifier("/Audio/_RMC14/Effects/ob_alert.ogg");
    [DataField]
    public SoundSpecifier TravelSound = new SoundPathSpecifier("/Audio/_RMC14/Weapons/gun_orbital_travel.ogg");
    [DataField]
    public SoundSpecifier ArrivalSound = new SoundCollectionSpecifier("RMCExplosionBig");
}
