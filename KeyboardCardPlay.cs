using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace KeyboardQuickPlay;

/// <summary>
/// 键盘出牌处理 - Harmony Patch 处理空格/Enter 键快速出牌
/// </summary>
[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class KeyboardCardPlayPatch
{
    /// <summary>
    /// Postfix：在原始 _Input 方法执行后检测空格/Enter 键
    /// </summary>
    static void Postfix(NMouseCardPlay __instance, InputEvent inputEvent)
    {
        // 原有逻辑：收到热键事件时触发
        // 新增逻辑：任意事件进来时，如果热键物理按住也触发（解决跨回合 echo 中断）
        if (!ModConfig.IsMatch(inputEvent) && !ModConfig.IsHeld())
            return;

        if (TryPlayWithKeyboard(__instance))
        {
            __instance.GetViewport()?.SetInputAsHandled();
        }
    }

    /// <summary>
    /// 尝试使用键盘快速出牌
    /// </summary>
    private static bool TryPlayWithKeyboard(NMouseCardPlay cardPlay)
    {
        // 获取卡牌（通过 Publicize 直接访问）
        var card = cardPlay.Card;
        if (card == null) return false;

        var targetType = card.TargetType;

        // 检查是否可以直接打出
        if (IsAutoPlayType(targetType))
        {
            // 对于单目标类型，获取血量最低的目标
            Creature target = null;
            if (targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly)
            {
                target = TargetSelector.GetBestTarget(card, targetType);
            }

            // 取消鼠标拖拽的异步流程，防止出牌后 LerpToMouse 报错
            cardPlay._cancellationTokenSource?.Cancel();

            // 设置目标并尝试出牌（通过 Publicize 直接访问）
            cardPlay._target = target;
            cardPlay.TryPlayCard(target);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否应该自动出牌
    /// </summary>
    private static bool IsAutoPlayType(TargetType targetType)
    {
        return targetType switch
        {
            TargetType.None => true,
            TargetType.Self => true,
            TargetType.AllEnemies => true,
            TargetType.RandomEnemy => true,
            TargetType.AllAllies => true,
            TargetType.Osty => true,
            TargetType.AnyEnemy => true,
            TargetType.AnyAlly => true,
            _ => false
        };
    }
}

/// <summary>
/// 记录出牌目标（所有出牌方式：鼠标/键盘/控制器）
/// </summary>
[HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.TryPlayCard))]
public static class FocusTargetPatch
{
    static void Postfix(Creature target)
    {
        TargetSelector.RecordTarget(target);
    }
}

/// <summary>
/// 战斗结束或被清理时清空集火记录
/// </summary>
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.EndCombatInternal))]
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.Reset))]
public static class CombatResetPatch
{
    static void Postfix()
    {
        TargetSelector.Reset();
    }
}

/// <summary>
/// 战斗开始时也确保清空旧的集火记录
/// </summary>
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.SetUpCombat))]
public static class CombatStartPatch
{
    static void Postfix()
    {
        TargetSelector.Reset();
    }
}
