using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace KeyboardQuickPlay.Handlers;
public partial class GameOverHandler : Node
{
    private NGameOverScreen _screen;

    public void Init(NGameOverScreen screen)
    {
        _screen = screen;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_screen))
            return;

        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;
  
        if (key.Keycode == Key.Space)
        {
            if (_screen._continueButton.IsEnabled)
            {
                var button = _screen._continueButton;
                button.GrabFocus();
                button.EmitSignal(NClickableControl.SignalName.Released, button);
            }
            else if (_screen._mainMenuButton.IsEnabled)
            {
                var button = _screen._mainMenuButton;
                button.GrabFocus();
                button.EmitSignal(NClickableControl.SignalName.Released, button);
            }
        }
    }
}
