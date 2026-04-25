using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace KeyboardQuickPlay.Handlers;

public partial class EventInputHandler : Node
{
    private NEventRoom _room;
    public void Init(NEventRoom room)
    {
        _room = room;
        ProcessMode = ProcessModeEnum.Always;
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_room))
            return;

        if (@event is not InputEventKey keyEvent)
            return;

        if (!keyEvent.Pressed || keyEvent.Echo)
            return;

        var options = _room.Layout.OptionButtons.ToList();
        
        if (options.Count == 0)
            return;

        if (keyEvent.Keycode == Key.Space)
        {
            if (options.Count == 1)
            {
                Trigger(options.ElementAt(0));
                return;
            }
        }

        int index = Helpers.KeyToIndex(keyEvent.Keycode);
        if (index < 0 || index >= options.Count)
            return;

        Trigger(options.ElementAt(index));
    }

    #region Helpers

    private static void Trigger(NEventOptionButton option)
    {
        if (!option.IsEnabled)
            return;

        option.GrabFocus();
        option.OnPress();
        option.OnRelease();
    }

    #endregion
}
