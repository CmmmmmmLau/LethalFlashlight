using System;
using System.Collections;
using LethalFlashlight.Patches;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LethalFlashlight.Object;

public class FlashlightTimer : MonoBehaviour{
    public bool isFlicking = false;
    
    private FlashlightItem parentFlashlight;
    private int flashlithyType;
    
    private void Start() {
        this.parentFlashlight = this.gameObject.GetComponentInParent<FlashlightItem>();
        flashlithyType = this.parentFlashlight.flashlightTypeID;
        this.StartCoroutine(Timer());
    }
    
    public IEnumerator Flicker() {
        Debug.Log("Flickering!");
        this.isFlicking = true;
        yield return new WaitForSeconds(Random.Range(1f,3f));
        this.isFlicking = false;
    }

    public IEnumerator Timer() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(45, 75));

            if (!this.parentFlashlight.flashlightBulb.enabled && !this.parentFlashlight.usingPlayerHelmetLight) continue;
            if (this.parentFlashlight.playerHeldBy.insanityLevel > 30f) {
                if (Random.Range(0f, 1f) < 0.3f) {
                    if (this.parentFlashlight.playerHeldBy.insanityLevel >= 40f) {
                        this.parentFlashlight.flashlightAudio.PlayOneShot(this.parentFlashlight.flashlightFlicker);
                    }
                }
                this.StartCoroutine(Flicker());
            } else if ((this.parentFlashlight.insertedBattery.charge <= FlashlightItemPatch.SPOTANGLE_THRESHOLD[flashlithyType])) {
                if (Random.Range(0f, 1f) < 0.2f) {
                    if (this.parentFlashlight.playerHeldBy.insanityLevel >= 40f) {
                        this.parentFlashlight.flashlightAudio.PlayOneShot(this.parentFlashlight.flashlightFlicker);
                    }
                }
            }
        }
    }
}