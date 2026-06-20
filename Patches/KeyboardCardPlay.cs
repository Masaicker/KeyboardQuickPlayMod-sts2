using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class KeyboardCardPlayPatch
{
    static void Postfix(NMouseCardPlay __instance, InputEvent inputEvent)
    {
        // Handle C key for cycling enemy target
        if (inputEvent is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.C)
        {
            var card = __instance.Card;
            var state = card?.CombatState;
            if (card == null || state == null)
                return;

            Creature target = null;
            if (card.TargetType == TargetType.AnyEnemy || card.TargetType == TargetType.AllEnemies)
                target = TargetSelector.Cycle(TargetSelector.GetValidEnemies(state));
            else if (card.TargetType == TargetType.AnyAlly || card.TargetType == TargetType.AllAllies)
                target = TargetSelector.Cycle(TargetSelector.GetValidAllies(state, card.Owner.Creature));

            TargetSelector.GetValidEnemies(state);
            __instance.CardNode?.SetPreviewTarget(target);
            __instance.GetViewport()?.SetInputAsHandled();
            return;
        }

        // Handle Space to play selected card
        if (!ModConfig.IsMatch(inputEvent) && !ModConfig.IsHeld())
            return;

        if (TryPlayWithKeyboard(__instance))
        {
            __instance.GetViewport()?.SetInputAsHandled();
        }
    }

    private static bool TryPlayWithKeyboard(NMouseCardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (card == null) 
            return false;

        var targetType = card.TargetType;

        Creature target = null;

        if (targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly)
            target = TargetSelector.CurrentTarget;
            
        if (target == null || !target.IsHittable)
            target = TargetSelector.GetBestTarget(card, targetType);

        cardPlay._cancellationTokenSource?.Cancel();
        cardPlay._target = target;

        cardPlay.TryPlayCard(target);
        return true;
    }
}

[HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.TryPlayCard))]
public static class FocusTargetPatch
{
    static void Postfix(Creature target)
    {
        TargetSelector.SetTarget(target);
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.EndCombatInternal))]
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.Reset))]
public static class CombatResetPatch
{
    static void Postfix()
    {
        TargetSelector.Reset();
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.SetUpCombat))]
public static class CombatStartPatch
{
    static void Postfix()
    {
        TargetSelector.Reset();
    }
}