using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace KeyboardQuickPlay;

[ModInitializer(nameof(Initialize))]
public static class Plugin
{
    private const string ModId = "Mhz.keyboardquickplay";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfig.Load();

        try
        {
            Harmony harmony = new(ModId);
            harmony.PatchAll();

            // 强制遍历确保 patch 生效（部分环境下 PatchAll 可能延迟绑定）
            var patched = harmony.GetPatchedMethods().ToList();
            Logger.Info($"Patch 挂载数量: {patched.Count}");
            foreach (var method in patched)
                Logger.Info($"Patch 已挂载: {method.DeclaringType?.Name}.{method.Name}");

            if (patched.Count == 0)
                Logger.Error("警告: 没有任何 Patch 成功挂载!");
        }
        catch (Exception e)
        {
            Logger.Error($"PatchAll 异常: {e}");
        }
    }
}