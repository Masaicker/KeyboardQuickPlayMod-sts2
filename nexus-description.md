[size=6][b]KeyboardQuickPlay[/b][/size]

[quote]选中卡牌后按空格键快速出牌，支持自定义按键（键盘/鼠标）。[/quote]

[size=5][b]功能特性[/b][/size]

[list]
[*][b]一键出牌[/b]：选中卡牌后按空格键，无需鼠标拖拽或点击目标；按住不放可在选牌时连续打出
[*][b]智能场景[/b]：无需选择目标，单一目标、全体攻击等场景瞬间打出
[*][b]智能目标[/b]：自动攻击上次攻击的敌人，首次攻击或目标死亡时选血量最低的敌人；队友技能默认选血量最低的队友
[*][b]自定义按键[/b]：支持键盘按键和鼠标按键，通过配置文件修改
[*][b]无缝兼容[/b]：不影响原有鼠标操作
[/list]

[size=5][b]安装[/b][/size]

1. 将 [b]KeyboardQuickPlay.dll[/b] 和 [b]KeyboardQuickPlay.pck[/b] 复制到游戏目录下的 [b]mods[/b] 文件夹：

[code]
Slay the Spire 2/
└── mods/
    ├── KeyboardQuickPlay.dll
    └── KeyboardQuickPlay.pck
[/code]

2. 启动游戏，MOD 会自动加载

[size=5][b]自定义按键[/b][/size]

首次启动后会自动生成配置文件 [b]mods/config/KeyboardQuickPlay.json[/b]：

[code]
{
    "QuickPlayButton": "Space"
}
[/code]

修改 [b]QuickPlayButton[/b] 的值即可更换按键，支持：
[list]
[*][b]键盘[/b]：Space、F、G、Tab、Enter 等
[*][b]鼠标[/b]：Xbutton1（侧键后退）、Xbutton2（侧键前进）、Middle（中键）等
[/list]

[size=5][b]技术实现[/b][/size]

[list]
[*]使用 Harmony 库进行运行时 Patch
[*]支持通过 JSON 配置文件自定义按键
[/list]

[size=5][b]致谢[/b][/size]

本项目基于 [url=https://github.com/Alchyr/ModTemplate-StS2]Alchyr/ModTemplate-StS2[/url] 模板创建。

特别感谢玩家 [url=https://www.nexusmods.com/profile/Kishibukukuuuu]Kishibukukuuuu[/url] 协助排查跨环境兼容性问题。

[size=5][b]许可证[/b][/size]

本项目遵循原游戏的 MOD 许可政策。

[size=5][b]源代码[/b][/size]

源代码托管于 [url=https://github.com/Masaicker/KeyboardQuickPlayMod-sts2]GitHub[/url]。


[size=6][b]KeyboardQuickPlay[/b][/size]

[quote]Select a card and press Space to play it instantly. Supports custom key bindings (keyboard/mouse).[/quote]

[size=5][b]Features[/b][/size]

[list]
[*][b]One-Click Play[/b]: Select a card and press Space — no mouse dragging or clicking required; hold the key to play cards continuously as you select them
[*][b]Smart Scenarios[/b]: Instantly plays cards with no target, single target, or AoE attacks
[*][b]Smart Targeting[/b]: Auto-attacks the last targeted enemy; selects lowest-HP enemy on first attack or when the target dies. Ally skills default to the lowest-HP ally
[*][b]Custom Key Binding[/b]: Supports keyboard keys and mouse buttons via config file
[*][b]Seamless Integration[/b]: Doesn't affect original mouse controls
[/list]

[size=5][b]Installation[/b][/size]

1. Copy [b]KeyboardQuickPlay.dll[/b] and [b]KeyboardQuickPlay.pck[/b] to the [b]mods[/b] folder in your game directory:

[code]
Slay the Spire 2/
└── mods/
    ├── KeyboardQuickPlay.dll
    └── KeyboardQuickPlay.pck
[/code]

2. Launch the game — the mod will load automatically

[size=5][b]Custom Key Binding[/b][/size]

A config file [b]mods/config/KeyboardQuickPlay.json[/b] is auto-generated on first launch:

[code]
{
    "QuickPlayButton": "Space"
}
[/code]

Change the [b]QuickPlayButton[/b] value to rebind:
[list]
[*][b]Keyboard[/b]: Space, F, G, Tab, Enter, etc.
[*][b]Mouse[/b]: Xbutton1 (side back), Xbutton2 (side forward), Middle (middle click), etc.
[/list]

[size=5][b]Technical Details[/b][/size]

[list]
[*]Uses the Harmony library for runtime patching
[*]Supports custom key binding via JSON config file
[/list]

[size=5][b]Acknowledgments[/b][/size]

This project is based on the [url=https://github.com/Alchyr/ModTemplate-StS2]Alchyr/ModTemplate-StS2[/url] template.

Special thanks to player [url=https://www.nexusmods.com/profile/Kishibukukuuuu]Kishibukukuuuu[/url] for helping debug cross-environment compatibility issues.

[size=5][b]License[/b][/size]

This project follows the original game's modding policy.

[size=5][b]Source Code[/b][/size]

Source code hosted on [url=https://github.com/Masaicker/KeyboardQuickPlayMod-sts2]GitHub[/url].
