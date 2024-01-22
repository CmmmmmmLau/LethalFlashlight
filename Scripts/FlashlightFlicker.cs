using System.Collections;
using LethalFlashlight.Components;
using Unity.Netcode;
using UnityEngine;

namespace LethalFlashlight.Scripts;

public class FlashlightFlicker: NetworkBehaviour {
    private FlashlightItem parentFlashlight;
    private FlashlightRework flashlightComponent;
    
    private void Start() {
        this.parentFlashlight = this.gameObject.GetComponentInParent<FlashlightItem>();
        this.flashlightComponent = this.gameObject.GetComponentInParent<FlashlightRework>();
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void UpdateStateServerRpc(bool state) {
        Plugin.mls.LogInfo("Sync flicker state Server RPC");
        this.UpdateStateClientRpc(state);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(bool state) {
        Plugin.mls.LogInfo("Received flicker sync client RPC");
        this.flashlightComponent.flag = state;
    }
}