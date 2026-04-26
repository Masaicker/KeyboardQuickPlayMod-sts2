using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NTreasureRoom), "_Ready")]
public static class TreasureRoomPatch
{
    static void Postfix(NTreasureRoom __instance)
    {
        var handler = new TreasureRoomHandler(__instance);
        __instance.AddChild(handler);
    }
}
