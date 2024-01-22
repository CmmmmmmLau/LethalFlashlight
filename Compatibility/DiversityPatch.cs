using Diversity.Items;
using UnityEngine;

namespace LethalFlashlight.Compatibility;

public class DiversityPatch {
    public static void RemoveComponent(FlashlightItem item) {
        if (!item.gameObject.TryGetComponent(out FlashlightRevamp remove)) return;
        Object.Destroy(remove);
        Plugin.mls.LogInfo("Removed FlashlightRevamp from flashlight");
    }
}