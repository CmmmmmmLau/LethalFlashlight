using BepInEx.Bootstrap;
using HarmonyLib;
using LethalFlashlight.Components;
using LethalFlashlight.Scripts;
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
            CompatibilityPatch.RemoveComponent(__instance);
        }
        
        FlashlightRework rework = __instance.gameObject.GetComponent<FlashlightRework>(); 
        if (!(bool) (Object) rework) {
            __instance.gameObject.AddComponent<FlashlightRework>();
            Plugin.mls.LogInfo("Added FlashlightRework to flashlight");
        }
    }
    
    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void FlashlightItemPatcher(FlashlightItem __instance) {
        
    }
}