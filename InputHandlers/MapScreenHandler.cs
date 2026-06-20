

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace KeyboardQuickPlay.Handlers;
public partial class MapScreenHandler : Node
{
    private NMapScreen _screen;

    public void Init(NMapScreen screen)
    {
        _screen = screen;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_screen))
            return;

        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;


        int index = Helpers.KeyToIndex(key.Keycode);
        
        if (index < 0)
            return;

        if (!_screen.IsTravelEnabled)
            return;

        var mapDict = Traverse.Create(_screen)
            .Field<Dictionary<MapCoord, NMapPoint>>("_mapPointDictionary")
            .Value;

        if (mapDict == null || mapDict.Count == 0)
            return;

        var rooms = mapDict.Values
            .Where(p => p != null && p.IsTravelable && p.Visible)
            .ToList();

        if (rooms.Count == 0)
            return;

        if (index >= rooms.Count)
            return;

        var room = rooms[index];

        room.TryGrabFocus();
        room.OnPress();
        room.OnRelease();
    }
}
