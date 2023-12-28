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

    private static float[] DEFAULT_FLASHLIGHT_INTENSITY = new float[2]{486.8536f, 397.9603f};
    private static float[] DEFAULT_FLASHLIGHT_SPOTANGLE = new float[2]{73f, 55.4f};
    
    private static float[] DEFAULT_HELMET_LIGHT_INTENSITY = new float[2]{486.8536f, 833.2255f};
    private static float[] DEFAULT_HELMET_LIGHT_SPOTANGLE = new float[2]{73f, 55.4f};

    private static bool isFlicking = false;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    static void flashlightTimerPatch(ref FlashlightItem __instance) {
        RefWrapper<FlashlightItem> instance = new RefWrapper<FlashlightItem>(__instance);
        __instance.StartCoroutine(FlashlightFlicker(instance));
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
            if (instanceRef.IsOwner) {
                Debug.Log("Owner Found!");
            }
            else {
                Debug.Log("Owner Not Found!");
            }
            float waitTime = (instanceRef.flashlightTypeID + 1) * 60;
            Debug.Log("Waiting for Flicker! Type: " + instanceRef.flashlightTypeID + " | " + waitTime + "s");
            yield return new WaitForSeconds(waitTime);
            if (instanceRef.flashlightBulb.enabled || instanceRef.usingPlayerHelmetLight) {
                float flickerChance = Random.Range(0.0f, 1.0f);
                float flickerChanceThreshold = 0.5f;
                
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