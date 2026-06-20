using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), "_Input")]
public static class CharacterSelectInputPatch
{
    static void Postfix(NCharacterSelectScreen __instance, InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        if (!Helpers.IsTopScreen(__instance))
            return;

        int index = Helpers.KeyToIndex(key.Keycode);

        if (index >= 0)
        {
            var container = AccessTools.Field(typeof(NCharacterSelectScreen), "_charButtonContainer")
                .GetValue(__instance) as Control;

            if (container == null)
                return;

            var buttons = container
                .GetChildren()
                .OfType<NCharacterSelectButton>()
                .Where(b => b.Visible)
                .ToList();

            if (index >= buttons.Count)
                return;

            var button = buttons[index];

            if (button.IsLocked)
                return;

            __instance.SelectCharacter(button, button.Character);
            button.GrabFocus();

            return;
        }

        // =========================
        // ⏎ / E → Embark
        // =========================
        if (key.Keycode is Key.Enter or Key.KpEnter or Key.E)
        {
            var embarkButton = AccessTools.Field(typeof(NCharacterSelectScreen), "_embarkButton")
                .GetValue(__instance) as NConfirmButton;

            if (embarkButton != null && embarkButton.IsVisible() && embarkButton.IsEnabled)
            {
                embarkButton.GrabFocus();
                embarkButton.EmitSignal(NClickableControl.SignalName.Released, embarkButton);
            }

            return;
        }

        if (key.Keycode is Key.Left)
        {
            var leftArrow = AccessTools.Field(typeof(NCharacterSelectScreen), "_leftArrow")
                .GetValue(__instance) as NButton;

            if (leftArrow != null && leftArrow.IsVisible() && leftArrow.IsEnabled)
            {
                leftArrow.EmitSignal(NClickableControl.SignalName.Released, leftArrow);
            }

            return;
        }

        if (key.Keycode is Key.Right)
        {
            var rightArrow = AccessTools.Field(typeof(NCharacterSelectScreen), "_rightArrow")
                .GetValue(__instance) as NButton;

            if (rightArrow != null && rightArrow.IsVisible() && rightArrow.IsEnabled)
            {
                rightArrow.EmitSignal(NClickableControl.SignalName.Released, rightArrow);
            }

            return;
        }

        if (key.Keycode is Key.Up)
        {
            var ascensionPanel = AccessTools.Field(typeof(NCharacterSelectScreen), "_ascensionPanel")
                .GetValue(__instance) as NAscensionPanel;

            ascensionPanel.IncrementAscension();
            return;
        }
        
        if (key.Keycode is Key.Down)
        {
            var ascensionPanel = AccessTools.Field(typeof(NCharacterSelectScreen), "_ascensionPanel")
                .GetValue(__instance) as NAscensionPanel;

            ascensionPanel.DecrementAscension();
            return;
        }
    }
}
