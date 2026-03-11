# KeyboardQuickPlay

选中卡牌后按空格键快速出牌，支持自定义按键（键盘/鼠标）。

## 功能特性

- **一键出牌**：选中卡牌后按空格键，无需鼠标拖拽或点击目标
- **智能场景**：无需选择目标，单一目标、全体攻击等场景瞬间打出
- **默认选择**：多目标时自动选血量最低（最优选仍需手动选择）
- **自定义按键**：支持键盘按键和鼠标按键，通过配置文件修改
- **无缝兼容**：不影响原有鼠标操作

## 安装

1. 将 `KeyboardQuickPlay.dll` 和 `KeyboardQuickPlay.pck` 复制到游戏目录下的 `mods` 文件夹：

   ```
   Slay the Spire 2/
   └── mods/
       ├── KeyboardQuickPlay.dll
       └── KeyboardQuickPlay.pck
   ```

2. 启动游戏，MOD 会自动加载

## 自定义按键

首次启动后会自动生成配置文件 `mods/config/KeyboardQuickPlay.json`：

```json
{
  "QuickPlayButton": "Space"
}
```

修改 `QuickPlayButton` 的值即可更换按键，支持：
- **键盘**：`Space`、`F`、`G`、`Tab`、`Enter` 等
- **鼠标**：`Xbutton1`（侧键后退）、`Xbutton2`（侧键前进）、`Middle`（中键）等

## 技术实现

- 使用 Harmony 库进行运行时 Patch
- 支持通过 JSON 配置文件自定义按键

## 致谢

本项目基于 [Alchyr/ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2) 模板创建。

## 许可证

本项目遵循原游戏的 MOD 许可政策。

## 支持一下

如果你觉得这个项目对你有帮助，欢迎在 Ko-fi 给我买杯咖啡 ☕
<p>
  <a href="https://ko-fi.com/masaicker">
    <img src="https://cdn.prod.website-files.com/5c14e387dab576fe667689cf/670f5a0171bfb928b21a7e00_support_me_on_kofi_beige.png" alt="Buy me a coffee" width="200">
  </a>
</p>

---

# KeyboardQuickPlay

Select a card and press Space to play it instantly. Supports custom key bindings (keyboard/mouse).

## Features

- **One-Click Play**: Select a card and press Space — no mouse dragging or clicking required
- **Smart Scenarios**: Instantly plays cards with no target, single target, or AoE attacks
- **Default Targeting**: Auto-selects lowest-HP in multi-target scenarios (manual selection optimal)
- **Custom Key Binding**: Supports keyboard keys and mouse buttons via config file
- **Seamless Integration**: Doesn't affect original mouse controls

## Installation

1. Copy `KeyboardQuickPlay.dll` and `KeyboardQuickPlay.pck` to the `mods` folder in your game directory:

   ```
   Slay the Spire 2/
   └── mods/
       ├── KeyboardQuickPlay.dll
       └── KeyboardQuickPlay.pck
   ```

2. Launch the game, the mod will load automatically

## Custom Key Binding

A config file `mods/config/KeyboardQuickPlay.json` is auto-generated on first launch:

```json
{
  "QuickPlayButton": "Space"
}
```

Change the `QuickPlayButton` value to rebind:
- **Keyboard**: `Space`, `F`, `G`, `Tab`, `Enter`, etc.
- **Mouse**: `Xbutton1` (side back), `Xbutton2` (side forward), `Middle` (middle click), etc.

## Technical Details

- Uses Harmony library for runtime patching
- Supports custom key binding via JSON config file

## Acknowledgments

This project is based on the [Alchyr/ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2) template.

## License

This project follows the original game's modding policy.

## Support

If you found this project useful, you're welcome to buy me a coffee on Ko-fi ☕

<p>
  <a href="https://ko-fi.com/masaicker">
    <img src="https://cdn.prod.website-files.com/5c14e387dab576fe667689cf/670f5a0171bfb928b21a7e00_support_me_on_kofi_beige.png" alt="Buy me a coffee" width="200">
  </a>
</p>
