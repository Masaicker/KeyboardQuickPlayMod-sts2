using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using KeyboardQuickPlay.Handlers;

[HarmonyPatch(typeof(NSingleplayerSubmenu), "_Ready")]
public static class SingleplayerSubmenu_AddHandler_Patch
{
    static void Postfix(NSingleplayerSubmenu __instance)
    {
        if (__instance.GetNodeOrNull<SingleplayerSubmenuInputHandler>("QuickInput") != null)
            return;

        var handler = new SingleplayerSubmenuInputHandler();
        handler.Name = "QuickInput";
        handler.Init(__instance);

        __instance.AddChild(handler);
    }
}
