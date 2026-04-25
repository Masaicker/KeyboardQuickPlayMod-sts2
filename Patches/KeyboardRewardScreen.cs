using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NRewardsScreen), "SetRewards")]
public static class NRewardsScreen_AddHotkeyControl
{
    public static void Postfix(NRewardsScreen __instance)
    {
        var node = new RewardHandler();
        node.Name = "RewardHandler";

        __instance.AddChild(node);
        node.Init(__instance);
    }
}