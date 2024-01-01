using System;
using System.Collections;
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
            yield return new WaitForSeconds(50f);

            if (!this.parentFlashlight.flashlightBulb.enabled && !this.parentFlashlight.usingPlayerHelmetLight) continue;
            if (true) {
                this.StartCoroutine(Flicker());
            }

        }
    }
}