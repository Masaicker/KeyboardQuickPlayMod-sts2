# 智能出牌 MOD (Smart Card Play) 开发规格文档

## MOD 名称
**Smart Card Play** (智能出牌)

## 项目概述

在 Slay the Spire 2 中，当玩家用鼠标拖拽手牌进入出牌区域后，如果牌需要选择目标，当前必须用鼠标点击具体目标。本 MOD 允许玩家按空格键（或 Enter 键）快速打出牌，系统会智能选择最佳目标。

## 功能需求

### 核心功能
当玩家用鼠标拖拽手牌进入出牌区域后：
1. 按 `Space`（空格）或 `Enter` 键 → 智能选择目标并打出牌
2. 按 `Esc` 键 → 取消出牌

### 智能目标选择策略

| TargetType | 行为 | 智能策略 |
|-----------|------|---------|
| `None` | 直接打出 | 无需目标 |
| `Self` | 直接打出 | 目标是自己 |
| `Osty` | 直接打出 | 目标是搭档 |
| `AllEnemies` | 直接打出 | 目标是所有敌人 |
| `AllAllies` | 直接打出 | 目标是所有队友 |
| `AnyEnemy`（1个敌人） | 直接打出 | 唯一目标 |
| `AnyEnemy`（多个敌人） | 智能选择 | 1. 有金光的目标（额外收益）<br>2. 血量最低的敌人<br>3. 从左到右 |
| `AnyAlly`（1个队友） | 直接打出 | 唯一目标 |
| `AnyAlly`（多个队友） | 智能选择 | 1. 有金光的目标（额外收益）<br>2. 血量最低的队友<br>3. 从左到右 |

### 金光目标（额外收益）示例

| 牌名 | 目标条件 | 额外效果 |
|-----|---------|---------|
| Dominate | 目标有 Vulnerable | 每层 Vulnerable 提供 1 点 Strength |
| Dismantle | 目标有 Vulnerable | 造成额外伤害 |
| BubbleBubble | 目标有 Poison | 基于 Poison 层数造成伤害 |

## 技术架构

### 修改的文件

#### 1. 新增文件
```
Mods/SmartCardPlay/
├── SmartCardPlayMod.cs           # MOD 主类
├── KeyboardCardPlay.cs           # 键盘出牌逻辑
└── SmartTargetSelector.cs        # 智能目标选择器
```

#### 2. Hook 点
通过 Harmony Patch 以下方法：
- `NMouseCardPlay.TargetSelection()` - 添加键盘输入监听
- `NMouseCardPlay._Input()` - 处理空格/Enter 键

### 核心代码结构

```csharp
// SmartTargetSelector.cs - 智能目标选择器
public static class SmartTargetSelector
{
    public static Creature? GetBestTarget(CardModel card, TargetType targetType)
    {
        var combatState = card.CombatState;
        if (combatState == null) return null;

        return targetType switch
        {
            TargetType.None or TargetType.Self or TargetType.AllEnemies
                or TargetType.AllAllies or TargetType.Osty => null,
            TargetType.AnyEnemy => GetBestEnemyTarget(card, combatState),
            TargetType.AnyAlly => GetBestAllyTarget(card, combatState),
            _ => null
        };
    }

    private static Creature? GetBestEnemyTarget(CardModel card, CombatState combatState)
    {
        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];

        // 1. 优先金光目标
        var goldTarget = enemies.FirstOrDefault(e => ShouldGlowGoldFor(card, e));
        if (goldTarget != null) return goldTarget;

        // 2. 血量最低
        return enemies.OrderBy(e => e.Hp).First();
    }

    private static Creature? GetBestAllyTarget(CardModel card, CombatState combatState)
    {
        var owner = card.Owner.Creature;
        var allies = combatState.PlayerCreatures
            .Where(c => c.IsHittable && c != owner)
            .ToList();

        if (allies.Count == 0) return null;
        if (allies.Count == 1) return allies[0];

        // 1. 优先金光目标
        var goldTarget = allies.FirstOrDefault(a => ShouldGlowGoldFor(card, a));
        if (goldTarget != null) return goldTarget;

        // 2. 血量最低
        return allies.OrderBy(a => a.Hp).First();
    }

    private static bool ShouldGlowGoldFor(CardModel card, Creature target)
    {
        // 检查牌对目标是否有额外收益
        // 通过反射访问 ShouldGlowGoldInternal 或直接检查常见组合
        if (target.HasPower<VulnerablePower>())
        {
            var cardId = card.Id.Entry.ToLower();
            if (cardId.Contains("dominate") || cardId.Contains("dismantle"))
                return true;
        }
        if (target.HasPower<PoisonPower>())
        {
            var cardId = card.Id.Entry.ToLower();
            if (cardId.Contains("bubblebubble"))
                return true;
        }
        return false;
    }
}
```

```csharp
// KeyboardCardPlay.cs - 键盘出牌逻辑
public static class KeyboardCardPlay
{
    // 在 NMouseCardPlay.TargetSelection() 中注册
    public static void RegisterKeyboardHandler(NMouseCardPlay cardPlay)
    {
        // 具体实现通过 Harmony Patch
    }

    // 处理空格/Enter 键
    public static bool TryPlayWithSmartTarget(NMouseCardPlay cardPlay)
    {
        if (!IsInPlayZone(cardPlay)) return false;

        var card = GetCard(cardPlay);
        if (card == null) return false;

        var targetType = card.TargetType;

        // 检查是否可以直接打出
        if (ShouldAutoPlay(card, targetType))
        {
            var target = SmartTargetSelector.GetBestTarget(card, targetType);
            TryPlayCard(cardPlay, target);
            return true;
        }

        return false;
    }
}
```

```csharp
// SmartCardPlayMod.cs - MOD 主类
public class SmartCardPlayMod
{
    public const string MOD_ID = "smart_card_play";
    public const string MOD_NAME = "Smart Card Play";
    public const string VERSION = "1.0.0";

    public static void Initialize()
    {
        // 注册 Harmony Patch
        var harmony = new Harmony(MOD_ID);

        // Patch NMouseCardPlay._Input 添加空格键处理
        harmony.Patch(
            original: AccessTools.Method(typeof(NMouseCardPlay), "_Input"),
            postfix: new HarmonyMethod(typeof(KeyboardCardPlay), "OnInput")
        );
    }
}
```

## Harmony Patch 实现

### Patch NMouseCardPlay._Input

```csharp
[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class NMouseCardPlay_Input_Patch
{
    static void Postfix(NMouseCardPlay __instance, InputEvent inputEvent)
    {
        // 检查是否在目标选择阶段
        if (!IsInTargetSelection(__instance)) return;

        // 检查是否按了空格或 Enter
        if (inputEvent is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed &&
                (inputEventKey.Keycode == Key.Space ||
                 inputEventKey.Keycode == Key.Enter ||
                 inputEventKey.Keycode == Key.KpEnter))
            {
                // 尝试智能出牌
                if (KeyboardCardPlay.TryPlayWithSmartTarget(__instance))
                {
                    GetViewport().SetInputAsHandled();
                }
            }
        }
        else if (inputEvent.IsActionPressed(MegaInput.accept))
        {
            // accept 输入也触发智能出牌
            if (KeyboardCardPlay.TryPlayWithSmartTarget(__instance))
            {
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
```

## 配置选项

### 未来可扩展的配置项

```ini
[SmartCardPlay]
# 启用智能出牌
Enabled = true

# 智能目标策略
# - lowest_hp: 优先血量最低
# - highest_hp: 优先血量最高
# - leftmost: 最左边
# - gold_only: 只对金光目标自动出牌
TargetStrategy = lowest_hp

# 是否显示智能选择提示
ShowTargetHint = true
```

## 测试场景

### 测试用例

1. **无需目标的牌**
   - Defend / Self buff 牌 → 按空格直接打出

2. **单敌人场景**
   - 任何攻击牌 → 按空格直接打唯一敌人

3. **多敌人场景**
   - 普通攻击牌 → 打血量最低的敌人
   - Dominate（有敌人带 Vulnerable）→ 打带 Vulnerable 的敌人
   - BubbleBubble（有敌人带 Poison）→ 打带 Poison 的敌人

4. **多队友场景**
   - Coordinate（加 Strength）→ 打血量最低的队友
   - DemonicShield（加护盾）→ 打血量最低的队友

5. **取消出牌**
   - 按 Esc 取消出牌

## MOD 信息

```
MOD ID: smart_card_play
名称: Smart Card Play
版本: 1.0.0
作者: [你的名字]
描述: 按空格键智能出牌，自动选择最佳目标
依赖: Slay the Spire 2
兼容性: 与其他 MOD 无冲突
```

## 构建说明

### 项目结构
```
SmartCardPlay/
├── SmartCardPlay.csproj
├── Mods/
│   └── SmartCardPlay/
│       ├── SmartCardPlayMod.cs
│       ├── KeyboardCardPlay.cs
│       └── SmartTargetSelector.cs
└── publish.ps1
```

### 依赖项
- HarmonyX (>= 2.2)
- Godot 4.5.1 SDK

### 构建步骤
```bash
dotnet build
# 或使用 publish.ps1
./publish.ps1
```

### 输出
```
mods/smart_card_play/
├── smart_card_play.dll
└── mod.json
```

# 源代码分析与验算

## 关键源代码位置

### 游戏源码位置
```
D:\zd\ModEditor\sts2\libs\Slay the Spire 2\src\Core\
```

### 核心类文件

| 文件路径 | 作用 | 关键方法/属性 |
|---------|------|--------------|
| `Nodes\Combat\NMouseCardPlay.cs` | 鼠标出牌逻辑 | `TargetSelection()`, `_Input()`, `SingleCreatureTargeting()` |
| `Nodes\Combat\NControllerCardPlay.cs` | 控制器出牌逻辑 | `SingleCreatureTargeting()`, `MultiCreatureTargeting()` |
| `Nodes\Combat\NCardPlay.cs` | 出牌基类 | `TryPlayCard()`, `CenterCard()` |
| `Nodes\Combat\NTargetManager.cs` | 目标管理器 | `StartTargeting()`, `AllowedToTargetCreature()` |
| `Nodes\Combat\NPlayerHand.cs` | 手牌管理 | `StartCardPlay()`, `_UnhandledInput()` |
| `Nodes\CommonUi\NInputManager.cs` | 输入管理 | `_UnhandledInput()`, `GetShortcutKey()` |
| `Entities\Cards\TargetType.cs` | 目标类型枚举 | `None, Self, AnyEnemy, AllEnemies, AnyAlly, AllAllies, Osty` |
| `Models\Cards\Dominate.cs` | 示例：Dominate 牌 | `ShouldGlowGoldInternal` |
| `Models\Cards\DeadlyPoison.cs` | 示例：DeadlyPoison 牌 | `OnPlay()` |

### 关键代码验证

#### 1. NMouseCardPlay.TargetSelection() 流程验证

**源码位置**: `D:\zd\ModEditor\sts2\libs\Slay the Spire 2\src\Core\Nodes\Combat\NMouseCardPlay.cs` 第 193-209 行

```csharp
// 原始代码
private async Task TargetSelection(TargetMode targetMode)
{
    if (base.Card != null)
    {
        TryShowEvokingOrbs();
        base.CardNode?.CardHighlight.AnimFlash();
        TargetType targetType = base.Card.TargetType;
        if ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false)
        {
            await SingleCreatureTargeting(targetMode, base.Card.TargetType);  // 单目标选择
        }
        else
        {
            await MultiCreatureTargeting(targetMode);  // 多目标（无需选择）
        }
    }
}
```

**验证点**:
- `TargetType.AnyEnemy` 或 `AnyAlly` → 进入 `SingleCreatureTargeting()` → 需要玩家点击目标
- 其他类型 → 进入 `MultiCreatureTargeting()` → 按下鼠标即可打出

#### 2. TargetType 枚举验证

**源码位置**: `D:\zd\ModEditor\sts2\libs\Slay the Spire 2\src\Core\Entities\Cards\TargetType.cs`

```csharp
public enum TargetType
{
    None,           // 无目标
    Self,           // 自己
    AnyEnemy,       // 单个敌人
    AllEnemies,     // 所有敌人
    RandomEnemy,    // 随机敌人
    AnyPlayer,      // 单个玩家
    AnyAlly,        // 单个队友
    AllAllies,      // 所有队友
    TargetedNoCreature, // 无生物目标
    Osty            // 搭档
}
```

#### 3. ShouldGlowGoldInternal 验证

**源码位置**: `D:\zd\ModEditor\sts2\libs\Slay the Spire 2\src\Core\Models\Cards\Dominate.cs` 第 19 行

```csharp
// Dominate: 如果敌人有 Vulnerable，牌会发金光
protected override bool ShouldGlowGoldInternal =>
    base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;
```

**其他示例**:
- `BubbleBubble`: 检查目标是否有 `PoisonPower`
- `Dismantle`: 检查目标是否有 `VulnerablePower`

#### 4. NControllerCardPlay 单目标流程验证

**源码位置**: `D:\zd\ModEditor\sts2\libs\Slay the Spire 2\src\Core\Nodes\Combat\NControllerCardPlay.cs` 第 86-126 行

```csharp
private async Task SingleCreatureTargeting(TargetType targetType)
{
    // ... 省略前面的代码 ...

    List<Creature> list = new List<Creature>();
    switch (targetType)
    {
    case TargetType.AnyEnemy:
        list = (from c in owner.CombatState.GetOpponentsOf(owner)
            where c.IsHittable
            select c).ToList();
        break;
    case TargetType.AnyAlly:
        list = base.Card.CombatState.PlayerCreatures.Where((Creature c) => c.IsHittable && c != owner).ToList();
        break;
    }
    if (list.Count == 0)
    {
        CancelPlayCard();
        return;
    }
    // 关键：即使只有 1 个目标，也需要玩家按 Enter 确认
    NCombatRoom.Instance.RestrictControllerNavigation(list.Select((Creature c) => NCombatRoom.Instance.GetCreatureNode(c).Hitbox));
    NCombatRoom.Instance.GetCreatureNode(list.First()).Hitbox.TryGrabFocus();
    NCreature nCreature = (NCreature)(await targetManager.SelectionFinished());
    // ...
}
```

**验证点**: 控制器模式即使只有 1 个目标，也需要手动确认，这是我们需要改进的地方。

## Harmony Patch 验算

### Patch 点选择

#### 方案 A: Patch NMouseCardPlay._Input()

```csharp
[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class NMouseCardPlay_Input_Patch
{
    static void Postfix(NMouseCardPlay __instance, InputEvent inputEvent)
    {
        // 检查是否在目标选择阶段
        // 需要通过反射访问私有字段来判断状态
    }
}
```

**问题**: `_Input()` 方法在 `StartCardPlay()` 之前也会调用，需要判断是否在目标选择阶段。

#### 方案 B: Patch NMouseCardPlay.SingleCreatureTargeting()

```csharp
[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay.SingleCreatureTargeting))]
public static class NMouseCardPlay_SingleCreatureTargeting_Patch
{
    static bool Prefix(NMouseCardPlay __instance, TargetMode targetMode, TargetType targetType)
    {
        // 如果按了空格键，直接跳过原始方法
        if (WasSpaceKeyPressed())
        {
            var card = GetCard(__instance);
            var target = SmartTargetSelector.GetBestTarget(card, targetType);
            TryPlayCard(__instance, target);
            return false;  // 跳过原始方法
        }
        return true;  // 正常执行
    }
}
```

**问题**: 需要在 `Prefix` 中检测按键，但 `InputEvent` 没有传进来。

#### 方案 C: 同时 Patch _Input() 和 SingleCreatureTargeting()

```csharp
// 1. 在 _Input() Postfix 中记录空格键状态
[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class NMouseCardPlay_Input_Patch
{
    static bool _spaceWasPressed = false;

    static void Postfix(NMouseCardPlay __instance, InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey key && key.Pressed &&
            (key.Keycode == Key.Space || key.Keycode == Key.Enter))
        {
            _spaceWasPressed = true;

            // 如果已经在目标选择阶段，直接打出
            if (IsInTargetSelection(__instance))
            {
                var card = GetCard(__instance);
                var target = SmartTargetSelector.GetBestTarget(card, card.TargetType);
                TryPlayCard(__instance, target);
                GetViewport().SetInputAsHandled();
                _spaceWasPressed = false;
            }
        }
    }
}
```

**最终方案**: 使用方案 C，在 `_Input()` 中直接处理空格键。

## 数据结构验证

### Creature 属性访问

```csharp
// 验证 Creature 类有以下可用的属性和方法
public class Creature
{
    public int Hp { get; }                    // ✅ 血量
    public bool IsAlive { get; }              // ✅ 是否存活
    public bool IsHittable { get; }           // ✅ 是否可被攻击
    public bool IsPlayer { get; }             // ✅ 是否是玩家
    public Side Side { get; }                 // ✅ 阵营
    public Vector2 Position { get; }          // ✅ 位置
    public CombatState CombatState { get; }   // ✅ 战斗状态

    public bool HasPower<T>() where T : Power;  // ✅ 检查是否有某种 Power
}
```

### CombatState 属性访问

```csharp
// 验证 CombatState 类
public class CombatState
{
    public IReadOnlyList<Creature> HittableEnemies { get; }  // ✅ 可攻击的敌人
    public IEnumerable<Creature> PlayerCreatures { get; }    // ✅ 玩家生物
}
```

### CardModel 属性访问

```csharp
// 验证 CardModel 类
public class CardModel
{
    public TargetType TargetType { get; }      // ✅ 目标类型
    public Player Owner { get; }              // ✅ 拥有者
    public Creature Creature { get; }         // ✅ 生物
    public CombatState CombatState { get; }   // ✅ 战斗状态
    public CardId Id { get; }                 // ✅ 牌 ID
}
```

## 测试验证方法

### 1. 单元测试结构

```csharp
[TestFixture]
public class SmartTargetSelectorTests
{
    [Test]
    public void GetBestTarget_SingleEnemy_ReturnsThatEnemy()
    {
        // Arrange
        var card = CreateMockCard(TargetType.AnyEnemy);
        var combatState = CreateMockCombatState(enemyCount: 1);

        // Act
        var target = SmartTargetSelector.GetBestTarget(card, TargetType.AnyEnemy);

        // Assert
        Assert.IsNotNull(target);
        Assert.AreEqual(combatState.HittableEnemies[0], target);
    }

    [Test]
    public void GetBestTarget_MultipleEnemies_ReturnsLowestHp()
    {
        // Arrange
        var card = CreateMockCard(TargetType.AnyEnemy);
        var enemies = new[] {
            CreateMockCreature(hp: 50),
            CreateMockCreature(hp: 10),  // 最低
            CreateMockCreature(hp: 30)
        };

        // Act
        var target = SmartTargetSelector.GetBestTarget(card, TargetType.AnyEnemy);

        // Assert
        Assert.AreEqual(10, target.Hp);
    }
}
```

### 2. 游戏内测试步骤

1. 启动游戏，进入战斗
2. 测试各种 TargetType 的牌：
   - Defend (Self) → 按空格 → 应该直接打出
   - Strike (AnyEnemy) 单敌人 → 按空格 → 应该直接打出
   - Strike (AnyEnemy) 多敌人 → 按空格 → 应该打血量最低的
   - Dominate 有 Vulnerable 敌人 → 按空格 → 应该打有 Vulnerable 的
3. 按 Esc 应该取消出牌

### 3. 日志输出验证

```csharp
// 在关键位置添加日志
public static Creature? GetBestTarget(CardModel card, TargetType targetType)
{
    Log.Info($"[SmartCardPlay] Getting best target for {card.Id.Entry} with type {targetType}");

    var target = /* ... */;

    if (target != null)
        Log.Info($"[SmartCardPlay] Selected target: {target.Name} (HP: {target.Hp})");
    else
        Log.Info($"[SmartCardPlay] No target selected (null)");

    return target;
}
```

## 开发时间线

1. ✅ 完成规格文档（含源代码分析与验算）
2. ⏳ 创建项目结构
3. ⏳ 实现 SmartTargetSelector
4. ⏳ 实现 KeyboardCardPlay
5. ⏳ 实现 Harmony Patch
6. ⏳ 测试各种 TargetType
7. ⏳ 构建和发布
