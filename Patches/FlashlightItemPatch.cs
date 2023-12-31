using System.Reflection;
using HarmonyLib;
using LethalFlashlight.Network;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Patches;

public class FlashlightItemPatch {
    private static float[] DEFAULT_FLASHLIGHT_INTENSITY = new float[2]{486.8536f, 397.9603f};
    private static float[] DEFAULT_FLASHLIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] DEFAULT_HELMET_LIGHT_INTENSITY = new float[2]{486.8536f, 833.2255f};
    private static float[] DEFAULT_HELMET_LIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    

    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void FlashlightItemPatcher(FlashlightItem __instance) {
        if (!(__instance.flashlightBulb.enabled || __instance.usingPlayerHelmetLight)) return;
        
        float targetBattery = __instance.usingPlayerHelmetLight ? __instance.playerHeldBy.pocketedFlashlight.insertedBattery.charge: __instance.insertedBattery.charge;

        float batteryValue = Mathf.Lerp(0.0f, 1.0f, targetBattery);
        float maxIntensity = __instance.flashlightTypeID switch {
            0 => batteryValue > 0.4f ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryValue / 0.4f), 0.2f),
            1 => batteryValue > 0.6f ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryValue / 0.6f), 0.1f),
            _ => 1
        };
        
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) {
            FlashlightNetworkHandler.Instance.IntensityEventClientRpc(__instance.NetworkObject, maxIntensity);
        } else {
            FlashlightNetworkHandler.Instance.IntensityEventServerRpc(__instance.NetworkObject, maxIntensity);
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
    
    static void SendEventToClients(NetworkObjectReference objectReference, float newIntensity) {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        Debug.Log("SendEventToClients Fired!");
        FlashlightNetworkHandler.Instance.IntensityEventClientRpc(objectReference, newIntensity);
    }
    
    static void ReceivedEventFromServer(NetworkObjectReference objectReference, float newIntensity) {
        Debug.Log("ReceivedEventFromServer Fired!");
        if (objectReference.TryGet(out NetworkObject networkObject)) {
            FlashlightItem flashlightItem = networkObject.gameObject.GetComponent<FlashlightItem>();
            int flashlightTypeID = flashlightItem.flashlightTypeID;
            if (!(flashlightItem.IsOwner) || flashlightItem.usingPlayerHelmetLight) {
                flashlightItem.playerHeldBy.helmetLight.intensity = DEFAULT_HELMET_LIGHT_INTENSITY[flashlightTypeID] * newIntensity;
            }
            else {
                flashlightItem.flashlightBulb.intensity = DEFAULT_FLASHLIGHT_INTENSITY[flashlightTypeID] * newIntensity;
            }
            Debug.Log("Intensity Changed! " + newIntensity);
        }
    }
}