using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace KeyboardQuickPlay.Handlers;

public partial class CardRewardHandler : Node
{
    private readonly NCardRewardSelectionScreen _screen;

    private Control _cardRow;

    public CardRewardHandler(NCardRewardSelectionScreen screen)
    {
        _screen = screen;
    }

    public override void _Ready()
    {
        GD.Print("[CardInput] Ready");

        SetProcessInput(true);

        var field = AccessTools.Field(typeof(NCardRewardSelectionScreen), "_cardRow");
        _cardRow = (Control)field?.GetValue(_screen);

        GD.Print($"[CardInput] cardRow found: {_cardRow != null}");

        if (_cardRow == null)
        {
            GD.PrintErr("[CardInput] ERROR: _cardRow is null");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_screen))
            return;

        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
            return;

        GD.Print($"[CardInput] Key: {keyEvent.Keycode}");

        // SPACE = skip
        if (keyEvent.Keycode == Key.Space)
        {
            GD.Print("[CardInput] Space pressed -> Skip");

            TriggerSkip();
            return;
        }

        if (!ModConfig.IsMatch(@event) && !ModConfig.IsHeld())
            return;

        int index = Helpers.KeyToIndex(keyEvent.Keycode);
        if (index < 0)
            return;

        var cards = GetCards();

        if (index >= cards.Count)
            return;

        GD.Print($"[CardInput] Selecting card {index}");

        cards[index].EmitSignal(NCardHolder.SignalName.Pressed, cards[index]);
    }

    private  List<NGridCardHolder> GetCards()
    {
        if (_cardRow == null)
        {
            GD.PrintErr("[CardInput] ERROR: _cardRow is null in GetCards");
            return new List<NGridCardHolder>();
        }

        var children = _cardRow.GetChildren();

        GD.Print($"[CardInput] Raw children count: {children.Count}");

        var cards = children
            .OfType<NGridCardHolder>()
            .ToList();

        GD.Print($"[CardInput] Filtered card holders: {cards.Count}");

        return cards;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent)
        {
            return;
        }

        GD.Print($"[CardInput] Key: {keyEvent.Keycode}, Pressed: {keyEvent.Pressed}, Echo: {keyEvent.Echo}");

        if (!keyEvent.Pressed || keyEvent.Echo)
        {
            GD.Print("[CardInput] Ignored (not pressed or echo)");
            return;
        }

        // if (!ModConfig.IsMatch(@event) && !ModConfig.IsHeld())
        // {
        //     GD.Print("[CardInput] ModConfig blocked input");
        //     return;
        // }

        int index = Helpers.KeyToIndex(keyEvent.Keycode);
        GD.Print($"[CardInput] Index resolved: {index}");

        if (index < 0)
        {
            GD.Print("[CardInput] Invalid key for selection");
            return;
        }

        var cards = GetCards();

        GD.Print($"[CardInput] Cards found: {cards.Count}");

        if (cards.Count == 0)
        {
            GD.PrintErr("[CardInput] ERROR: No cards found");
            return;
        }

        if (index >= cards.Count)
        {
            GD.Print($"[CardInput] Index {index} out of range");
            return;
        }

        GD.Print($"[CardInput] Selecting card at index {index}");

        // Preferred: simulate click
        cards[index].EmitSignal(NCardHolder.SignalName.Pressed, cards[index]);
    }

private void TriggerSkip()
{
    var field = AccessTools.Field(
        typeof(NCardRewardSelectionScreen),
        "_rewardAlternativesContainer"
    );

    var container = (Control)field?.GetValue(_screen);

    if (container == null)
    {
        GD.PrintErr("[CardInput] No reward alternatives container");
        return;
    }

    var buttons = container
        .GetChildren()
        .OfType<NCardRewardAlternativeButton>()
        .ToList();

    GD.Print($"[CardInput] Alt buttons: {buttons.Count}");

    if (buttons.Count == 0)
    {
        GD.Print("[CardInput] No skip option available");
        return;
    }

    var button = buttons[0];

    GD.Print("[CardInput] Calling OnPress()");

    button.Call(NCardRewardAlternativeButton.MethodName.OnPress);
}

}
