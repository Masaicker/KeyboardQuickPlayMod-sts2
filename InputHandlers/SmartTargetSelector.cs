using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KeyboardQuickPlay.Handlers;

public static class TargetSelector
{
    public static Creature CurrentTarget => _focusTarget;
    public static NCreature CurrentTargetNode => _focusNode;

    private static Creature _focusTarget;
    private static NCreature _focusNode;
    private static int _enemyIndex = 0;

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

    public static Creature SetTarget(Creature target)
    {
        // Deselect old node
        _focusNode?.HideSingleSelectReticle();

        _focusTarget = target;
        _focusNode = target != null
            ? NCombatRoom.Instance?.CreatureNodes.FirstOrDefault(n => n.Entity == target)
            : null;

        // Select new node
        _focusNode?.ShowSingleSelectReticle();

        return _focusTarget;
    }

    public static Creature Cycle(IReadOnlyList<Creature> list)
    {
        if (list == null || list.Count == 0)
            return null;

        if (_focusTarget == null || !list.Contains(_focusTarget))
            _enemyIndex = 0;
        else
            _enemyIndex = (list.IndexOf(_focusTarget) + 1) % list.Count;

        // SetTarget handles show/hide
        SetTarget(list[_enemyIndex]);
        return _focusTarget;
    }

    public static void Reset()
    {
        _focusNode?.HideSingleSelectReticle();
        _focusTarget = null;
        _focusNode = null;
        _enemyIndex = 0;
    }


    private static Creature GetEnemyTarget(ICombatState combatState)
    {
        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) 
            return null;
        if (enemies.Count == 1) 
            return enemies[0];

        if (_focusTarget != null && enemies.Contains(_focusTarget) && _focusTarget.IsHittable)
            return _focusTarget;

        return enemies.OrderBy(e => e.CurrentHp).First();
    }

    private static Creature GetAllyTargetWithLowestHp(CardModel card, ICombatState combatState)
    {
        var owner = card.Owner.Creature;
        var allies = combatState.PlayerCreatures
            .Where(c => c.IsHittable && c != owner)
            .ToList();

        if (allies.Count == 0) 
            return null;
        
        if (allies.Count == 1) 
            return allies[0];

        return allies.OrderBy(a => a.CurrentHp).First();
    }

    public static List<Creature> GetValidEnemies(ICombatState state)
        => state.HittableEnemies.Where(e => e.IsHittable).ToList();

    public static List<Creature> GetValidAllies(ICombatState state, Creature owner)
        => state.PlayerCreatures.Where(c => c.IsHittable && c != owner).ToList();
}