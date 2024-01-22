using BepInEx.Bootstrap;
using HarmonyLib;
using LethalFlashlight.Compatibility;
using LethalFlashlight.Components;
using UnityEngine;

namespace LethalFlashlight.Patches;

public class FlashlightItemPatch {

    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    [HarmonyAfter("Chaos.Diversity")]
    static void TimerPather(FlashlightItem __instance) {
        if (Chainloader.PluginInfos.ContainsKey("Chaos.Diversity")) {
            Plugin.mls.LogInfo("Plugin Diversity detected");
            DiversityPatch.RemoveComponent(__instance);
        }
    }
}