using System;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Network;

public class FlashlightNetworkHandler : NetworkBehaviour{
    public static FlashlightNetworkHandler Instance { get; private set; }
    public static event Action<NetworkObjectReference, float, float> IntensityChangedEvent;

    public override void OnNetworkSpawn() {
        IntensityChangedEvent = null;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) {
            Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
        }
        
        Instance = this;
        Debug.Log("NetworkHandler Spawned!");
        
        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void IntensityEventServerRpc(NetworkObjectReference refObject, float intensityMultiplier, float spotAngleMultiplier){
        IntensityEventClientRpc(refObject, intensityMultiplier, spotAngleMultiplier);
    }
    
    [ClientRpc]
    public void IntensityEventClientRpc(NetworkObjectReference refObject, float intensityMultiplier, float spotAngleMultiplier){
        IntensityChangedEvent?.Invoke(refObject, intensityMultiplier, spotAngleMultiplier);
    }
}