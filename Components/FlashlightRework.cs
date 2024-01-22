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

    private FlashlightItem parentFlashlight;
    private int type;
    public bool flag;

    private float targetTimer;
    private float timer;
    
    private void Start() {
        this.parentFlashlight = this.gameObject.GetComponent<FlashlightItem>();
        this.type = this.parentFlashlight.flashlightTypeID;
        this.flag = false;

        this.targetTimer = Plugin.FLICKER_INTERVAL;
        this.timer = 0;
    }

    private void Update() {
        if (this.parentFlashlight.isBeingUsed) {
            Battery targetBattery = this.parentFlashlight.usingPlayerHelmetLight
                ? this.parentFlashlight.playerHeldBy.pocketedFlashlight.insertedBattery
                : this.parentFlashlight.insertedBattery;

            if (!this.parentFlashlight.IsOwner) {
                targetBattery.charge -= Time.deltaTime / this.parentFlashlight.itemProperties.batteryUsage;
            } else {
                this.timer += Time.deltaTime;
                if (this.timer > this.targetTimer) {
                    if (((Object) this.parentFlashlight.playerHeldBy != (Object) null) && this.parentFlashlight.playerHeldBy.insanityLevel > 30f) {
                        if (Random.Range(0f, 1f) < Plugin.FLICKER_CHANCE_INSANITY) {
                            if (this.parentFlashlight.playerHeldBy.insanityLevel >= 40) {
                                this.parentFlashlight.flashlightAudio.PlayOneShot(this.parentFlashlight.flashlightFlicker);
                            }
                            this.StartCoroutine(SyncFlicking());
                        }
                    } else {
                        if (targetBattery.charge <= Plugin.FLASHLIGHT_THRESHOLD[this.type]) {
                            if (Random.Range(0f, 1f) < Plugin.FLICKER_CHANCE) {
                                this.StartCoroutine(SyncFlicking());
                            }
                        }
                    }
                    this.timer = 0;
                }
            }
            
            float batteryPercentage = targetBattery.charge > 1 ? 1: Mathf.Lerp(0.0f, 1.0f, targetBattery.charge);
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
            }
            else {
                if (targetBattery.charge > 0) {
                    this.LightTweaker(this.parentFlashlight.flashlightBulb, Plugin.FLASHLIGHT_INTENSITY[type], multiplier);
                } 
            }
        } else {
            if (!this.parentFlashlight.isHeld) {
                if (this.parentFlashlight.insertedBattery.charge <= 0) {
                    this.parentFlashlight.SwitchFlashlight(false);
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
    
    [ServerRpc(RequireOwnership = true)]
    public void UpdateStateServerRpc(bool state) {
        Plugin.mls.LogInfo("Server RPC: syncing flicker");
        this.UpdateStateClientRpc(state);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(bool state) {
        Plugin.mls.LogInfo("Client RPC: Received new flicker state");
        this.flag = state;
    }
}