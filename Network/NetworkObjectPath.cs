using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Network;

public class NetworkObjectPath {
    public static GameObject networkPrefab;

    [HarmonyPatch(typeof(GameNetworkManager))]
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Initialize() {
        if (networkPrefab != null) return;

        networkPrefab = Plugin.MainAssetBundle.LoadAsset<GameObject>("FlashlightNetworkHandler");
        networkPrefab.AddComponent<FlashlightNetworkHandler>();
        
        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        
        Debug.Log("NetworkObjectPath Initialized!");
    }

    [HarmonyPatch(typeof(StartOfRound))]
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void SpawnNetworkPrefab() {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) {
            var networkHandler = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
            networkHandler.GetComponent<NetworkObject>().Spawn();
        }
    }
}