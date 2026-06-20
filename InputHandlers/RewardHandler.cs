using Godot;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace KeyboardQuickPlay.Handlers;

public partial class RewardHandler : Node
{
    private NRewardsScreen _screen;

    public void Init(NRewardsScreen screen)
    {
        _screen = screen;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_screen))
            return;

        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
            return;

        var buttons = GetRewardButtons();
        if (buttons.Count == 0)
            return;

        if (keyEvent.Keycode == Key.Space)
        {
            Trigger(buttons[0]);
            return;
        }

        int index = Helpers.KeyToIndex(keyEvent.Keycode);
        if (index < 0 || index >= buttons.Count)
            return;

        Trigger(buttons[index]);
    }

    private List<NRewardButton> GetRewardButtons()
        => _screen._rewardButtons.OfType<NRewardButton>().ToList();

    private static void Trigger(NRewardButton button)
    {
        button.ForceClick();
    }
}