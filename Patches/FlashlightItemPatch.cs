using BepInEx.Bootstrap;
using HarmonyLib;
using LethalFlashlight.Compatibility;
using LethalFlashlight.Components;
using UnityEngine;

namespace LethalFlashlight.Patches;

[HarmonyPatch(typeof(FlashlightItem))]
public class FlashlightItemPatch {

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    [HarmonyAfter("Chaos.Diversity")]
    private static void TimerPather(FlashlightItem __instance) {
        if (Chainloader.PluginInfos.ContainsKey("Chaos.Diversity")) {
            Plugin.mls.LogInfo("Plugin Diversity detected");
            DiversityPatch.RemoveComponent(__instance);
        }
    }
    
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePather(FlashlightItem __instance) {
        FlashlightRework component = __instance.gameObject.GetComponent<FlashlightRework>();
        if ((Object)component != (Object)null) {
            component.IntensityUpdate();
        }
    }

    [HarmonyPatch("ItemActivate")]
    [HarmonyPatch("PocketItem")]
    [HarmonyPatch("EquipItem")]
    [HarmonyPrefix]
    private static void UpdateCharge(FlashlightItem __instance) {
        FlashlightRework component = __instance.gameObject.GetComponent<FlashlightRework>();
        if ((Object) component == (Object) null) return;
        component.UpdateChargeServerRpc(component.charge);
    }
}