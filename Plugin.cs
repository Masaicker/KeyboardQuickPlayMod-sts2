using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace KeyboardQuickPlay;

[ModInitializer(nameof(Initialize))]
public static class Plugin
{
    private const string ModId = "Mhz.keyboardquickplay"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfig.Load();
        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}