using System;
using System.Collections;
using GameNetcodeStuff;
using LethalFlashlight.Patches;
using LethalFlashlight.Scripts;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LethalFlashlight.Components;

public class FlashlightRework : NetworkBehaviour{
    private static float[] DEFAULT_FLASHLIGHT_INTENSITY = new float[2]{486.8536f, 397.9603f};
    private static float[] DEFAULT_FLASHLIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] DEFAULT_HELMET_LIGHT_INTENSITY = new float[2]{486.8536f, 833.2255f};
    private static float[] DEFAULT_HELMET_LIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] INTENITY_THRESHOLD = new float[2]{0.4f, 0.6f};
    private static float[] INTENITY_MINIMUM = new float[2]{0.2f, 0.1f};
    
    public static float[] SPOTANGLE_THRESHOLD = new float[2]{0.4f, 0.6f};
    public static float[] SPOTANGLE_MINIMUM = new float[2]{0.9f, 0.8f};
    
    private FlashlightItem parentFlashlight;
    private int type;
    public bool flag;

    private float targetTimer;
    private float timer;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
    
    private void Start() {
        this.parentFlashlight = this.gameObject.GetComponent<FlashlightItem>();
        this.type = this.parentFlashlight.flashlightTypeID;
        this.flag = false;

        this.targetTimer = 10;
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
                    this.timer = 0;
                    this.StartCoroutine(Flicker());
                }
            }
            
            float batteryPercentage = Mathf.Lerp(0.0f, 1.0f, targetBattery.charge);
            float intensityMultiplier = batteryPercentage > INTENITY_THRESHOLD[type] ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryPercentage / INTENITY_THRESHOLD[type]), INTENITY_MINIMUM[type]);



            if (this.flag) {
                intensityMultiplier = Random.Range(0.1f, 0.2f);
            }
            
            if (this.parentFlashlight.isHeld) {
                if (!this.parentFlashlight.IsOwner || this.parentFlashlight.usingPlayerHelmetLight) {
                    this.parentFlashlight.playerHeldBy.helmetLight.intensity = DEFAULT_HELMET_LIGHT_INTENSITY[type] * intensityMultiplier;
                } else {
                    this.parentFlashlight.flashlightBulb.intensity = DEFAULT_FLASHLIGHT_INTENSITY[type] * intensityMultiplier;
                }
            }
            else {
                this.parentFlashlight.flashlightBulb.intensity = DEFAULT_FLASHLIGHT_INTENSITY[type] * intensityMultiplier;
            }
        }
    }
    
    public IEnumerator Flicker() {
        yield return new WaitUntil(() => NetworkObject.IsSpawned);
        this.parentFlashlight.gameObject.GetComponent<FlashlightFlicker>().UpdateStateServerRpc(true);
        yield return new WaitForSeconds(Random.Range(1f, 3));
        this.parentFlashlight.gameObject.GetComponent<FlashlightFlicker>().UpdateStateServerRpc(false);    
    } 
}