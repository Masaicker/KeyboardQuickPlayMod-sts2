# Changelog

# v1.0.2
* 修复存档重进或放弃游戏后，多人战斗时快速出牌导致的崩溃
* 优化目标锁定清理逻辑，防止引用失效
* Fix crash when quick playing cards after game restart or run abandonment
* Improve target reset logic to prevent stale references

# v1.0.1
* 新增智能目标选择：自动攻击上次目标，首次或目标死亡时选血量最低
* 修复快速出牌时偶发的拖拽异常
* Add smart targeting: auto-attacks last target, falls back to lowest-HP on first attack or target death
* Fix occasional drag error during quick card play

# v1.0.0
* 初始版本
* Initial release
