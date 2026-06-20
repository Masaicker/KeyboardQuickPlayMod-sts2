using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using System.Reflection;
using KeyboardQuickPlay.Handlers;

namespace KeyboardQuickPlay.Patches;

[HarmonyPatch(typeof(NMainMenu), "_Ready")]
public static class NMainMenu_Ready_Patch
{
    static void Postfix(NMainMenu __instance)
    {
        var handler = new MainMenuHandler();
        handler.Name = "HotkeyHandler";

        __instance.AddChild(handler);
        handler.Init(__instance);
    }
}