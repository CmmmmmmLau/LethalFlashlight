using Diversity.Items;
using UnityEngine;

namespace LethalFlashlight.Scripts;

public class CompatibilityPatch {
    public static void RemoveComponent(FlashlightItem item) {
        if (!item.gameObject.TryGetComponent(out FlashlightRevamp remove)) return;
        Object.Destroy(remove);
        Plugin.mls.LogInfo("Removed FlashlightRevamp from flashlight");
    }
}