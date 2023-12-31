using HarmonyLib;
using LethalFlashlight.Network;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Patches;

public class FlashlightItemPatch {

    [HarmonyPatch(typeof(FlashlightItem))]
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void FlashlightItemPatcher(FlashlightItem __instance) {
        if (__instance.flashlightBulb.enabled) {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) {
                Debug.Log("IntensityEventServerRpc Fired!");
                FlashlightNetworkHandler.Instance.IntensityEventClientRpc(__instance.NetworkObject, 1f);
            } else {
              Debug.Log("IntensityEventServerRpc Fired!");
              FlashlightNetworkHandler.Instance.IntensityEventServerRpc(__instance.NetworkObject, 1f);
            }
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
    }
}