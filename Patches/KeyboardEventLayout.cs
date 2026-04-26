using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KeyboardQuickPlay.Handlers;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NEventLayout), "AddOptions")]
public static class KeyboardEventLayoutPatch
{
    static void Postfix(NEventLayout __instance)
    {
        var node = new EventInputHandler();
        node.Init(__instance);
        __instance.AddChild(node);
    }
}
