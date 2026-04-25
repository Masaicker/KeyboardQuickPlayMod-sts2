using Godot;
using HarmonyLib;
using System;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace KeyboardQuickPlay.Handlers;

public partial class RestSiteHandler : Node
{
    private NRestSiteRoom _room;

    public void Init(NRestSiteRoom room)
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

        if (keyEvent.Keycode == Key.Space)
        {
            var proceed = _room.ProceedButton;

            if (proceed == null)
                return;

            if (proceed.IsEnabled)
            {
                var method = AccessTools.Method(typeof(NRestSiteRoom), "OnProceedButtonReleased");
                method?.Invoke(_room, new object[] { null });
            }

            return;
        }

        // Number keys → options
        int index = Helpers.KeyToIndex(keyEvent.Keycode);

        if (index < 0)
            return;

        var options = _room.Options;

        if (options == null) 
            return;

        if (index >= options.Count) 
            return;

        var buttons = _room._choicesContainer.GetChildren(false).OfType<NRestSiteButton>().Where(b => b.Visible);

        if (buttons.Count() < index) 
            return;

        var selectedButton = buttons.ElementAt(index);

        if (!selectedButton.IsEnabled)
            return;

        selectedButton.Call(NRestSiteButton.MethodName.OnPress);
        selectedButton.Call(NRestSiteButton.MethodName.OnRelease);
    }
}
