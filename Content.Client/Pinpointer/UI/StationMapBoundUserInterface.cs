using Content.Shared.Pinpointer;
using Robust.Client.UserInterface;
using Content.Shared._Impstation.Shuttles.Events; // imp
using Robust.Shared.Map; // imp

namespace Content.Client.Pinpointer.UI;

public sealed class StationMapBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private StationMapWindow? _window;

    public StationMapBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        EntityUid? gridUid = null;
        var clickCoords = false; // imp

        if (EntMan.TryGetComponent<StationMapComponent>(Owner, out var comp) && comp.TargetGrid != null)
        {
            gridUid = comp.TargetGrid;
            clickCoords = comp.SendClickCoords; // imp
        }
        else if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
        {
            gridUid = xform.GridUid;
        }

        _window = this.CreateWindow<StationMapWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

        _window.ToggleClickCoords(clickCoords); // imp
        _window.RequestClickCoord += OnClickCoordRequest; // imp

        string stationName = string.Empty;
        if(EntMan.TryGetComponent<MetaDataComponent>(gridUid, out var gridMetaData))
        {
            stationName = gridMetaData.EntityName;
        }

        if (comp != null && comp.ShowLocation)
            _window.Set(stationName, gridUid, Owner);
        else
            _window.Set(stationName, gridUid, null);
    }

    // imp start
    private void OnClickCoordRequest(MapCoordinates coords)
    {
        SendMessage(new ClickCoordMessage()
        {
            Coordinates = coords,
        });
    }
    // imp end
}
