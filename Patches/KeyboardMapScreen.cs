using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NMapScreen), "_Ready")]
public static class MapNumberSelectionPatch
{
    static void Postfix(NMapScreen __instance)
    {
        var handler = new MapScreenHandler();
        handler.Name = "MapScreenHandler";

        __instance.AddChild(handler);
        handler.Init(__instance);
    }
}
