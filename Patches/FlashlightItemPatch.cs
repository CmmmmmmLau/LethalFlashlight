using System.Reflection;
using HarmonyLib;
using LethalFlashlight.Network;
using LethalFlashlight.Object;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Patches;

public class FlashlightItemPatch {
    private static float[] DEFAULT_FLASHLIGHT_INTENSITY = new float[2]{486.8536f, 397.9603f};
    private static float[] DEFAULT_FLASHLIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] DEFAULT_HELMET_LIGHT_INTENSITY = new float[2]{486.8536f, 833.2255f};
    private static float[] DEFAULT_HELMET_LIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] INTENITY_THRESHOLD = new float[2]{0.4f, 0.6f};
    private static float[] INTENITY_MINIMUM = new float[2]{0.2f, 0.1f};
    
    private static float[] SPOTANGLE_THRESHOLD = new float[2]{0.4f, 0.6f};
    private static float[] SPOTANGLE_MINIMUM = new float[2]{0.8f, 0.6f};

    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void TimerPather(FlashlightItem __instance) {
        FlashlightTimer timer = __instance.gameObject.GetComponent<FlashlightTimer>(); 
        if (timer == null) {
            timer = __instance.gameObject.AddComponent<FlashlightTimer>();
        }
    }
    
    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void FlashlightItemPatcher(FlashlightItem __instance) {
        if (!(__instance.flashlightBulb.enabled || __instance.usingPlayerHelmetLight)) return;
        int type = __instance.flashlightTypeID;
        float targetBattery = __instance.usingPlayerHelmetLight ? __instance.playerHeldBy.pocketedFlashlight.insertedBattery.charge: __instance.insertedBattery.charge;
        
        FlashlightTimer timer = __instance.gameObject.GetComponent<FlashlightTimer>();

        float batteryPercentage = Mathf.Lerp(0.0f, 1.0f, targetBattery);
        float intensityMultiplier = batteryPercentage > INTENITY_THRESHOLD[type] ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryPercentage / INTENITY_THRESHOLD[type]), INTENITY_MINIMUM[type]);
        float spotAngleMultiplier = batteryPercentage > SPOTANGLE_THRESHOLD[type] ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryPercentage / SPOTANGLE_THRESHOLD[type]), SPOTANGLE_MINIMUM[type]);

        if (timer.isFlicking) {
            intensityMultiplier = Random.Range(0.1f, 0.3f);
        }
        
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) {
            FlashlightNetworkHandler.Instance.IntensityEventClientRpc(__instance.NetworkObject, intensityMultiplier, spotAngleMultiplier);
        } else {
            FlashlightNetworkHandler.Instance.IntensityEventServerRpc(__instance.NetworkObject, intensityMultiplier, spotAngleMultiplier);
        }
    }
    
    
    [HarmonyPatch(typeof(RoundManager))]
    [HarmonyPatch("GenerateNewFloor")]
    [HarmonyPostfix]
    static void SubscribeToHandler() {
        FlashlightNetworkHandler.IntensityChangedEvent -= ReceivedEventFromServer;
        FlashlightNetworkHandler.IntensityChangedEvent += ReceivedEventFromServer;
    }
    
    [HarmonyPatch(typeof(RoundManager))]
    [HarmonyPatch("DespawnPropsAtEndOfRound")]
    [HarmonyPostfix]
    static void UnsubscribeFromHandler() {
        FlashlightNetworkHandler.IntensityChangedEvent -= ReceivedEventFromServer;
    }
    
    static void SendEventToClients(NetworkObjectReference objectReference, float intensityMultiplier, float spotAngleMultiplier) {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        Debug.Log("SendEventToClients Fired!");
        FlashlightNetworkHandler.Instance.IntensityEventClientRpc(objectReference, intensityMultiplier, spotAngleMultiplier);
    }
    
    static void ReceivedEventFromServer(NetworkObjectReference objectReference, float intensityMultiplier, float spotAngleMultiplier) {
        Debug.Log("ReceivedEventFromServer Fired!");
        if (objectReference.TryGet(out NetworkObject networkObject)) {
            FlashlightItem flashlightItem = networkObject.gameObject.GetComponent<FlashlightItem>();
            int flashlightTypeID = flashlightItem.flashlightTypeID;
            if (!(flashlightItem.IsOwner) || flashlightItem.usingPlayerHelmetLight) {
                flashlightItem.playerHeldBy.helmetLight.intensity = DEFAULT_HELMET_LIGHT_INTENSITY[flashlightTypeID] * intensityMultiplier;
                flashlightItem.playerHeldBy.helmetLight.spotAngle = DEFAULT_HELMET_LIGHT_SPOTANGLE[flashlightTypeID] * spotAngleMultiplier;
            }
            else {
                flashlightItem.flashlightBulb.intensity = DEFAULT_FLASHLIGHT_INTENSITY[flashlightTypeID] * intensityMultiplier;
                flashlightItem.flashlightBulb.spotAngle = DEFAULT_FLASHLIGHT_SPOTANGLE[flashlightTypeID] * spotAngleMultiplier;
            }
        }
    }
}