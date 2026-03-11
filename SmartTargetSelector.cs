using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace KeyboardQuickPlay;

/// <summary>
/// 目标选择器 - 简单逻辑：血量最低
/// </summary>
public static class TargetSelector
{
    /// <summary>
    /// 获取指定牌和目标类型的最佳目标
    /// </summary>
    public static Creature GetBestTarget(CardModel card, TargetType targetType)
    {
        var combatState = card.CombatState;
        if (combatState == null) return null;

        return targetType switch
        {
            TargetType.None or TargetType.Self or TargetType.AllEnemies
                or TargetType.AllAllies or TargetType.Osty => null,
            TargetType.AnyEnemy => GetEnemyTargetWithLowestHp(combatState),
            TargetType.AnyAlly => GetAllyTargetWithLowestHp(card, combatState),
            _ => null
        };
    }

    /// <summary>
    /// 获取血量最低的敌人
    /// </summary>
    private static Creature GetEnemyTargetWithLowestHp(CombatState combatState)
    {
        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];

        return enemies.OrderBy(e => e.CurrentHp).First();
    }

    /// <summary>
    /// 获取血量最低的队友
    /// </summary>
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
