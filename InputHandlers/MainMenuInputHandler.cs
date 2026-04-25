

using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace KeyboardQuickPlay.Handlers;
public partial class MainMenuInputHandler : Node
{
    private NMainMenu _menu;

    public void Init(NMainMenu menu)
    {
        _menu = menu;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_menu))
            return;

        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        int index;    
        if (key.Keycode == Key.Space)
        {
            index = 0;
        }
        else
        {
            index = Helpers.KeyToIndex(key.Keycode);
        }

        if (index < 0)
            return;

        var buttons = GetButtons(_menu);
        if (buttons == null || index >= buttons.Length)
            return;

        var button = buttons[index];

        button.GrabFocus();
        button.EmitSignal(NClickableControl.SignalName.Released, button);
    }

    #region Reflection helper
    private static NButton[] GetButtons(NMainMenu menu)
    {
        var prop = typeof(NMainMenu).GetProperty(
            "MainMenuButtons",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        var buttons = prop?.GetValue(menu) as NButton[];

        return buttons.Where(b => b != null && b.IsVisible() && b.IsEnabled).ToArray();
    }
    #endregion
}
