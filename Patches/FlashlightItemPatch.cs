using System.Collections;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using Random = UnityEngine.Random;
using LC_API;

namespace LethalFlashlight.Patches;

public class RefWrapper<T>
{
    public T Value { get; set; }

    public RefWrapper(T value)
    {
        Value = value;
    }
}

[HarmonyPatch(typeof(FlashlightItem))]
internal class FlashlightItemPatch {

    private static float[] DEFAULT_HELMET_LIGHT_INTENSITY = new float[2];
    private static bool isFlicking = false;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void flashlightTimerPatch(ref FlashlightItem __instance) {
        RefWrapper<FlashlightItem> instance = new RefWrapper<FlashlightItem>(__instance);
        __instance.StartCoroutine(FlashlightFlicker(instance));
        if (DEFAULT_HELMET_LIGHT_INTENSITY[__instance.flashlightTypeID] == null) {
            DEFAULT_HELMET_LIGHT_INTENSITY[__instance.flashlightTypeID] = __instance.playerHeldBy.helmetLight.intensity;
        }
    }
    
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void flashlishtIntensityPatch(ref FlashlightItem __instance) {
        if (!(__instance.flashlightBulb.enabled || __instance.usingPlayerHelmetLight)) return;
        
        float targetBattery = __instance.usingPlayerHelmetLight ? __instance.playerHeldBy.pocketedFlashlight.insertedBattery.charge: __instance.insertedBattery.charge;

        float batteryValue = Mathf.Lerp(0.0f, 1.0f, targetBattery);
        float maxIntensity = __instance.flashlightTypeID switch {
            0 => batteryValue > 0.4f ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryValue / 0.4f), 0.2f),
            1 => batteryValue > 0.6f ? 1.0f : Mathf.Max(Mathf.Lerp(0.0f, 1.0f, batteryValue / 0.6f), 0.1f),
            _ => 1
        };

        if (__instance.flashlightBulb.enabled) {
            __instance.flashlightBulb.intensity *= maxIntensity;
        } else if (__instance.usingPlayerHelmetLight && !isFlicking) {
            __instance.playerHeldBy.helmetLight.intensity = DEFAULT_HELMET_LIGHT_INTENSITY[__instance.flashlightTypeID] * maxIntensity;
        }
    }
    
    private static IEnumerator FlashlightFlicker(RefWrapper<FlashlightItem> instance) {
        FlashlightItem instanceRef = instance.Value;
        Random.InitState(instanceRef.GetHashCode());
        Debug.Log("Initiated Random Seed: " + instanceRef.GetHashCode());
        while (true) {
            float waitTime = Random.Range(120f - 30 * (instanceRef.flashlightTypeID + 1), 120f - 30 * instanceRef.flashlightTypeID);
            Debug.Log("Waiting for Flicker! Type: " + instanceRef.flashlightTypeID + " | " + waitTime + "s");
            yield return new WaitForSeconds(waitTime);
            if (instanceRef.flashlightBulb.enabled || instanceRef.usingPlayerHelmetLight) {
                float maxChance = instanceRef.flashlightTypeID == 0 ? Mathf.Lerp(0.1f, 0.0f, instanceRef.insertedBattery.charge / 0.7f) 
                                                                        : Mathf.Lerp(0.1f, 0.0f, instanceRef.insertedBattery.charge / 0.9f);
                if (true) {
                    Debug.Log("Starting Flicker! Type: " + instanceRef.flashlightTypeID);
                    float defaultIntensity = instanceRef.usingPlayerHelmetLight? instanceRef.playerHeldBy.helmetLight.intensity : instanceRef.flashlightBulb.intensity;
                    float startTime = Time.realtimeSinceStartup;
                    float flickerTimer = Time.realtimeSinceStartup;
                    while (flickerTimer - startTime < Random.Range(1 ,3)) {
                        flickerTimer = Time.realtimeSinceStartup;
                        Debug.Log("Flicker Timer: " + (flickerTimer - startTime));
                        yield return new WaitForSeconds(0.1f);
                        if (instanceRef.flashlightBulb.enabled) {
                            instanceRef.flashlightInterferenceLevel = 1;
                            Debug.Log("Flicker Intensity: " + instance.Value.flashlightBulb.intensity);
                        } else if (instance.Value.usingPlayerHelmetLight) {
                            instanceRef.playerHeldBy.helmetLight.intensity = Random.Range(0.0f, defaultIntensity / 2);
                            Debug.Log("Flicker Intensity: " + instance.Value.playerHeldBy.helmetLight.intensity);
                        }
                    }
                    instance.Value.flashlightInterferenceLevel = 0;
                }
            }
        }
    }
}