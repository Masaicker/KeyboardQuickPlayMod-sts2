using HarmonyLib;
using KeyboardQuickPlay.Handlers;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NGameOverScreen), "_Ready")]
public static class NGameOverScreen_Ready_Patch
{
    public static void Postfix(NGameOverScreen __instance)
    {
        var node = new GameOverHandler
        {
            Name = "GameOverHandler"
        };

        __instance.AddChild(node);
        node.Init(__instance);
    }
}
