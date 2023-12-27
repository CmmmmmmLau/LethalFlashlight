using System;
using BepInEx;
using HarmonyLib;
using LethalFlashlight.Patches;
using UnityEngine;

namespace LethalFlashlight
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony("LethalFlashlight");
        private void Awake()
        {
            this.harmony.PatchAll(typeof(FlashlightItemPatch));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
