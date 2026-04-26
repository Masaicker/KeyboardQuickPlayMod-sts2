using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace KeyboardQuickPlay.Handlers;

public partial class SingleplayerSubmenuInputHandler : Node
{
    private NSingleplayerSubmenu _menu;

    public void Init(NSingleplayerSubmenu menu)
    {
        _menu = menu;
    }

    public override void _Input(InputEvent e)
    {
        if (_menu == null)
            return;

        if (e is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        if (!Helpers.IsTopScreen(_menu))
            return;

        int index = Helpers.KeyToIndex(key.Keycode);
        if (index < 0)
            return;

        var buttons = GetButtons();
        if (index >= buttons.Count)
            return;

        Trigger(buttons[index]);
    }

    #region Helpers

    private List<NSubmenuButton> GetButtons()
    {
        var standard = AccessTools.Field(typeof(NSingleplayerSubmenu), "_standardButton")
            .GetValue(_menu) as NSubmenuButton;

        var daily = AccessTools.Field(typeof(NSingleplayerSubmenu), "_dailyButton")
            .GetValue(_menu) as NSubmenuButton;

        var custom = AccessTools.Field(typeof(NSingleplayerSubmenu), "_customButton")
            .GetValue(_menu) as NSubmenuButton;

        return new[]
        {
            standard,
            daily,
            custom
        }
        .Where(b => b != null && b.Visible && b.IsInsideTree() && b.IsEnabled)
        .ToList();
    }

    private void Trigger(NSubmenuButton button)
    {
        if (button == null)
            return;

        button.GrabFocus();

        // 🔥 IMPORTANT: this menu uses Released, not Pressed
        button.EmitSignal(NClickableControl.SignalName.Released, button);
    }

    #endregion
}
