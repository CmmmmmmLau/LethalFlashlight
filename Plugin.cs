using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalFlashlight.Components;
using LethalFlashlight.Misc;
using LethalFlashlight.Patches;
using RuntimeNetcodeRPCValidator;
using UnityEngine;

namespace LethalFlashlight
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_GUID, RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("Chaos.Diversity", BepInDependency.DependencyFlags.SoftDependency)]
    
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource mls;
        public static Config cfg;
        
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private NetcodeValidator netcodeValidator;
        
        
        private void Awake()
        {
            mls = this.Logger;
            cfg = new Config(this.Config);
            
            
            harmony.PatchAll(typeof(FlashlightItemPatch));
            harmony.PatchAll(typeof(Config));

            netcodeValidator = new NetcodeValidator(PluginInfo.PLUGIN_GUID);
            netcodeValidator.PatchAll();
            
            netcodeValidator.BindToPreExistingObjectByBehaviour<FlashlightRework, FlashlightItem>();
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
