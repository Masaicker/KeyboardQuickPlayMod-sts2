using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace KeyboardQuickPlay;

/// <summary>
/// 目标选择器 - 集火优先，血量最低兜底
/// </summary>
public static class TargetSelector
{
    private static bool DebugLog = false;
    private static Creature _focusTarget;

    public static Creature GetBestTarget(CardModel card, TargetType targetType)
    {
        var combatState = card.CombatState;
        if (combatState == null) return null;

        return targetType switch
        {
            TargetType.AnyEnemy => GetEnemyTarget(combatState),
            TargetType.AnyAlly => GetAllyTargetWithLowestHp(card, combatState),
            _ => null
        };
    }

    /// <summary>
    /// 记录集火目标（快速出牌和手动出牌都会调用）
    /// </summary>
    public static void RecordTarget(Creature target)
    {
        if (target == null) return;
        var old = _focusTarget;
        _focusTarget = target;
        if (DebugLog)
            Plugin.Logger.Info($"[集火] 记录目标: {target.Name} (HP:{target.CurrentHp}/{target.MaxHp}) | 上一个: {(old != null ? old.Name : "无")}");
    }

    /// <summary>
    /// 清空集火记录（新战斗时调用）
    /// </summary>
    public static void Reset()
    {
        if (DebugLog)
            Plugin.Logger.Info($"[集火] 战斗开始/结束/重置时，清空目标! 之前: {(_focusTarget != null ? _focusTarget.Name : "无")}");
        _focusTarget = null;
    }

    private static Creature GetEnemyTarget(CombatState combatState)
    {
        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];

        // 集火优先：上次打过的目标还活着就继续打
        if (_focusTarget != null && enemies.Contains(_focusTarget) && _focusTarget.IsHittable)
        {
            if (DebugLog)
                Plugin.Logger.Info($"[集火] 复用目标: {_focusTarget.Name} (HP:{_focusTarget.CurrentHp}/{_focusTarget.MaxHp})");
            return _focusTarget;
        }

        if (DebugLog)
        {
            var reason = _focusTarget == null ? "无集火目标" : !enemies.Contains(_focusTarget) ? "目标不在列表中" : !_focusTarget.IsHittable ? "目标已死" : "未知";
            Plugin.Logger.Info($"[集火] 回退到最低血量 ({reason})");
        }

        // 兜底：血量最低
        return enemies.OrderBy(e => e.CurrentHp).First();
    }

    private static Creature GetAllyTargetWithLowestHp(CardModel card, CombatState combatState)
    {
        var owner = card.Owner.Creature;
        var allies = combatState.PlayerCreatures
            .Where(c => c.IsHittable && c != owner)
            .ToList();

        if (allies.Count == 0) return null;
        if (allies.Count == 1) return allies[0];

        return allies.OrderBy(a => a.CurrentHp).First();
    }
}
