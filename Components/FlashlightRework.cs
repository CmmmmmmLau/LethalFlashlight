using System;
using System.Collections;
using GameNetcodeStuff;
using LethalFlashlight.Patches;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LethalFlashlight.Components;

public class FlashlightRework : NetworkBehaviour{
    
    
    public float charge;
    public bool flag;
    
    private FlashlightItem parentFlashlight;
    private int type;
    private float targetTimer;
    private float timer;
    
    private void Start() {
        this.parentFlashlight = this.gameObject.GetComponent<FlashlightItem>();
        this.charge = this.parentFlashlight.insertedBattery.charge;
        this.type = this.parentFlashlight.flashlightTypeID;
        this.flag = false;

        this.targetTimer = Plugin.FLICKER_INTERVAL;
        this.timer = 0;
    }

    public void IntensityUpdate() {
        if ((Object) this.parentFlashlight == (Object) null) return;
        
        if (this.parentFlashlight.isBeingUsed) {
            Battery targetBattery = this.parentFlashlight.insertedBattery;
            
            float targetCharge = targetBattery.charge;

            if (!this.parentFlashlight.IsOwner) {
                targetCharge = this.charge;
                this.charge -=  Time.deltaTime / this.parentFlashlight.itemProperties.batteryUsage;
            } else {
                this.charge = targetCharge;
                this.timer += Time.deltaTime;
                if (this.timer > this.targetTimer) {
                    Plugin.mls.LogInfo("Flicker timer reached, Checking...");
                    if (((Object) this.parentFlashlight.playerHeldBy != (Object) null) && this.parentFlashlight.playerHeldBy.insanityLevel > 30f) {
                        if (Random.Range(0f, 1f) < Plugin.FLICKER_CHANCE_INSANITY) {
                            Plugin.mls.LogInfo("Flicker chance reached, Flickering...");
                            if (this.parentFlashlight.playerHeldBy.insanityLevel >= 40) {
                                this.parentFlashlight.flashlightAudio.PlayOneShot(this.parentFlashlight.flashlightFlicker);
                            }
                            this.StartCoroutine(SyncFlicking());
                        } else {
                          Plugin.mls.LogInfo("Flicker chance not reached, Skipping...");  
                        }
                    } else {
                        if (targetCharge <= Plugin.FLASHLIGHT_THRESHOLD[this.type]) {
                            if (Random.Range(0f, 1f) < Plugin.FLICKER_CHANCE) {
                                Plugin.mls.LogInfo("Flicker chance reached, Flickering...");
                                this.StartCoroutine(SyncFlicking());
                            } else {
                                Plugin.mls.LogInfo("Flicker chance not reached, Skipping...");  
                            }
                        }
                    }
                    this.timer = 0;
                }
            }
            
            float batteryPercentage = targetCharge > 1 ? 1: Mathf.Lerp(0.0f, 1.0f, targetCharge);
            float multiplier = batteryPercentage > Plugin.FLASHLIGHT_THRESHOLD[type] ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryPercentage / Plugin.FLASHLIGHT_THRESHOLD[type]), Plugin.FLASHLIGHT_INTENSITY_MIN[type]);

            
            if (this.flag) {
                multiplier = Random.Range(0.1f, 0.3f);
            }
            
            if (this.parentFlashlight.isHeld) {
                if (!this.parentFlashlight.IsOwner || this.parentFlashlight.usingPlayerHelmetLight) {
                    this.LightTweaker(this.parentFlashlight.playerHeldBy.helmetLight, Plugin.FLASHLIGHT_INTENSITY_HELMET[type], multiplier);
                } else {
                    this.LightTweaker(this.parentFlashlight.flashlightBulb, Plugin.FLASHLIGHT_INTENSITY[type], multiplier);
                }
            } else {
                if (targetCharge > 0) {
                    this.LightTweaker(this.parentFlashlight.flashlightBulb, Plugin.FLASHLIGHT_INTENSITY[type], multiplier);
                } 
            }
        }
    }
    
    private void LightTweaker(Light light, float intensity, float multiplier) {
        light.intensity = intensity * multiplier;
        light.range =
            Plugin.FLASHLIGHT_RANGE[type] * multiplier < Plugin.FLASHLIGHT_RANGE_MIN[type]
                ? Plugin.FLASHLIGHT_RANGE_MIN[type]
                : Plugin.FLASHLIGHT_RANGE[type] * multiplier;
        light.spotAngle = 
            Plugin.FLASHLIGHT_SPOTANGLE[type] * multiplier < Plugin.FLASHLIGHT_SPOTANGLE_MIN[type]
                ? Plugin.FLASHLIGHT_SPOTANGLE_MIN[type]
                : Plugin.FLASHLIGHT_SPOTANGLE[type] * multiplier;
    }
    
    public IEnumerator SyncFlicking() {
        yield return new WaitUntil(() => NetworkObject.IsSpawned);
        this.UpdateStateServerRpc(true);
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        this.UpdateStateServerRpc(false);    
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateStateServerRpc(bool state) {
        Plugin.mls.LogInfo("Server RPC: syncing flicker");
        this.UpdateStateClientRpc(state);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(bool state) {
        Plugin.mls.LogInfo("Client RPC: Received new flicker state");
        this.flag = state;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateChargeServerRpc(float charge) {
        Plugin.mls.LogInfo("Server RPC: syncing charge - " + charge.ToString() + " -");
        this.UpdateChargeClientRpc(charge);
    }
    
    [ClientRpc]
    private void UpdateChargeClientRpc(float charge) {
        if(this.parentFlashlight.IsOwner) return;
        Plugin.mls.LogInfo("Client RPC: Received new charge - " + charge.ToString() + " -");
        this.charge = charge;
    }
}