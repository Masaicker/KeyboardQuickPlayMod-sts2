using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Godot;

namespace KeyboardQuickPlay;

public static class ModConfig
{
    private const string ConfigDir = "mods/config";
    private const string ConfigFile = "KeyboardQuickPlay.json";

    private static Key? _key;
    private static MouseButton? _mouseButton;
    public static bool DebugLog = false;

    public static bool IsMatch(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey { Pressed: true } keyEvent)
        {
            if (_key.HasValue && keyEvent.Keycode == _key.Value)
                return true;

            // Fallback: 输入法可能导致 Keycode 为 None，用 PhysicalKeycode 兜底
            if (_key.HasValue && keyEvent.Keycode == Key.None && keyEvent.PhysicalKeycode == _key.Value)
                return true;
        }

        if (_mouseButton.HasValue && inputEvent is InputEventMouseButton { Pressed: true } mouseEvent && mouseEvent.ButtonIndex == _mouseButton.Value)
            return true;

        return false;
    }

    /// <summary>
    /// 轮询物理按键状态（不依赖事件流，解决跨回合 echo 中断问题）
    /// </summary>
    public static bool IsHeld()
    {
        if (_key.HasValue && Input.IsKeyPressed(_key.Value))
            return true;
        if (_mouseButton.HasValue && Input.IsMouseButtonPressed(_mouseButton.Value))
            return true;
        return false;
    }

    public static void Load()
    {
        var gameDir = Path.GetDirectoryName(OS.GetExecutablePath());
        if (gameDir != null)
        {
            var configDirPath = Path.Combine(gameDir, ConfigDir);
            var configFilePath = Path.Combine(configDirPath, ConfigFile);

            var config = ConfigData.Default;

            if (File.Exists(configFilePath))
            {
                try
                {
                    var json = File.ReadAllText(configFilePath);
                    config = JsonSerializer.Deserialize<ConfigData>(json) ?? ConfigData.Default;
                }
                catch (Exception e)
                {
                    Plugin.Logger.Error($"Failed to read config, using defaults: {e.Message}");
                    Save(configDirPath, configFilePath, config);
                }
            }
            else
            {
                Save(configDirPath, configFilePath, config);
            }

            ParseBinding(config, configDirPath, configFilePath);
        }
    }

    private static void ParseBinding(ConfigData config, string configDirPath, string configFilePath)
    {
        _key = null;
        _mouseButton = null;
        var binding = config.QuickPlayButton;

        if (Enum.TryParse<MouseButton>(binding, true, out var mb) && mb != MouseButton.None)
        {
            _mouseButton = mb;
            Plugin.Logger.Info($"Bound to mouse button: {mb}");
        }
        else if (Enum.TryParse<Key>(binding, true, out var key) && key != Key.None)
        {
            _key = key;
            Plugin.Logger.Info($"Bound to key: {key}");
        }
        else
        {
            Plugin.Logger.Error($"Unknown binding '{binding}', falling back to Space");
            _key = Key.Space;
            Save(configDirPath, configFilePath, ConfigData.Default);
        }
    }

    private static void Save(string configDirPath, string configFilePath, ConfigData config)
    {
        try
        {
            Directory.CreateDirectory(configDirPath);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configFilePath, json);
            Plugin.Logger.Info($"Default config saved to: {configFilePath}");
        }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Failed to save config: {e.Message}");
        }
    }

    public class ConfigData
    {
        public string QuickPlayButton { get; init; } = "Space";

        public string Comment_EN { get; init; } = "QuickPlayButton: Set the quick card play button. " +
            "Keyboard: A-Z, Key0-Key9, F1-F12, Space, Tab, Enter, CapsLock, Shift, Ctrl, Alt, Up, Down, Left, Right, Insert, Delete, Home, End, PageUp, PageDown, Backspace, Escape, etc. " +
            "Mouse: Left, Right, Middle, Xbutton1 (side back), Xbutton2 (side forward).";

        public string Comment_CN { get; init; } = "QuickPlayButton: 设置快速出牌的按键。" +
            "键盘: A-Z, Key0-Key9, F1-F12, Space(空格), Tab, Enter(回车), CapsLock(大写锁定), Shift, Ctrl, Alt, Up(上), Down(下), Left(左), Right(右), Insert, Delete, Home, End, PageUp, PageDown, Backspace(退格), Escape(ESC) 等。" +
            "鼠标: Left(左键), Right(右键), Middle(中键/滚轮), Xbutton1(侧键后退), Xbutton2(侧键前进)。";

        public static ConfigData Default => new();
    }
}
