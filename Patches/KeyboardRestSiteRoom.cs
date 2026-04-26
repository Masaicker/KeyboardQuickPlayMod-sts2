using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
public static class NRestSiteRoom_Ready_Patch
{
    public static void Postfix(NRestSiteRoom __instance)
    {
        if (!__instance.IsInsideTree())
            return;

        // Prevent duplicates
        if (__instance.GetNodeOrNull<RestSiteHandler>("RestSiteHotkeyNode") != null)
            return;

        var node = new RestSiteHandler
        {
            Name = "RestSiteHandler"
        };

        __instance.AddChild(node);
        node.Init(__instance);
    }
}
