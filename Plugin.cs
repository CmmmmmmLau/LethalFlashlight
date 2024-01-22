using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalFlashlight.Components;
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
        
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private NetcodeValidator netcodeValidator;

        private ConfigEntry<float> flashlightIntensityConfig;
        private ConfigEntry<float> flashlightIntensityHelmetConfig;
        private ConfigEntry<float> flashlightIntensityMinConfig;
        private ConfigEntry<float> flashlightRangeConfig;
        private ConfigEntry<float> flashlightRangeMinConfig;
        private ConfigEntry<float> flashlightSpotAngleConfig;
        private ConfigEntry<float> flashlightSpotAngleMinConfig;
        private ConfigEntry<float> flashlightThresholdConfig;
        
        private ConfigEntry<float> prolightIntensityConfig;
        private ConfigEntry<float> prolightIntensityHelmetConfig;
        private ConfigEntry<float> prolightIntensityMinConfig;
        private ConfigEntry<float> prolightRangeConfig;
        private ConfigEntry<float> prolightRangeMinConfig;
        private ConfigEntry<float> prolightSpotAngleConfig;
        private ConfigEntry<float> prolightSpotAngleMinConfig;
        private ConfigEntry<float> prolightThresholdConfig;
        
        private ConfigEntry<int> flickerIntervalConfig;
        private ConfigEntry<float> flickerChanceConfig;
        private ConfigEntry<float> flickerChanceinsanityConfig;
        
        public static float[] FLASHLIGHT_INTENSITY;
        public static float[] FLASHLIGHT_INTENSITY_HELMET;
        public static float[] FLASHLIGHT_INTENSITY_MIN;
        public static float[] FLASHLIGHT_RANGE;
        public static float[] FLASHLIGHT_RANGE_MIN;
        public static float[] FLASHLIGHT_SPOTANGLE;
        public static float[] FLASHLIGHT_SPOTANGLE_MIN;
        public static float[] FLASHLIGHT_THRESHOLD;
        
        public static int FLICKER_INTERVAL;
        public static float FLICKER_CHANCE;
        public static float FLICKER_CHANCE_INSANITY;
        
        
        private void Awake()
        {
            mls = this.Logger;

            this.flashlightIntensityConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Intensity", 397.9603f, "The default intensity of the Flashlight");
            this.flashlightIntensityHelmetConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Intensity Helmet", 833.2255f, "The default intensity of the Flashlight when used as a helmet light");
            this.flashlightIntensityMinConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Intensity Min", 0.1f, "The minimum intensity of the Flashlight(in percentage)");
            this.flashlightRangeConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Range", 17f, "The default range of the Flashlight");
            this.flashlightRangeMinConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Range Min", 17f, "The minimum range of the Flashlight");
            this.flashlightSpotAngleConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Spot Angle", 55.4f, "The default spot angle of the Flashlight");
            this.flashlightSpotAngleMinConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Spot Angle Min", 55.4f, "The minimum spot angle of the Flashlight");
            this.flashlightThresholdConfig = Config.Bind<float>("Flashlight Tweak", "Flashlight Threshold", 0.6f, "The percentage of the Flashlight start losing power");
 
            this.prolightIntensityConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Intensity", 486.8536f, "The default intensity of the ProFlashlight");
            this.prolightIntensityHelmetConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Intensity Helmet", 486.8536f, "The default intensity of the ProFlashlight when used as a helmet light");
            this.prolightIntensityMinConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Threshold Min", 0.2f, "The minimum threshold of the ProFlashlight");
            this.prolightRangeConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Range", 55f, "The default range of the ProFlashlight");
            this.prolightRangeMinConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Range Min", 30f, "The minimum range of the ProFlashlight");
            this.prolightSpotAngleConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Spot Angle", 73f, "The default spot angle of the ProFlashlight");
            this.prolightSpotAngleMinConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Spot Angle Min", 50f, "The minimum spot angle of the ProFlashlight");
            this.prolightThresholdConfig = Config.Bind<float>("ProFlashlight Tweak", "ProFlashlight Threshold", 0.5f, "The percentage of the ProFlashlight start losing power");

            this.flickerIntervalConfig = Config.Bind<int>("Flicker", "Flicker Interval", 60, "The interval of the Flashlight Flicker in seconds");
            this.flickerChanceConfig = Config.Bind<float>("Flicker", "Flicker Chance", 0.2f, "The chance of the Flashlight will flicking when the battery is low");
            this.flickerChanceinsanityConfig = Config.Bind<float>("Flicker", "Flicker Chance Insanity", 0.3f, "The chance of the Flashlight will flicking when the player's insanity is high");

            FLASHLIGHT_INTENSITY = new[] {prolightIntensityConfig.Value, flashlightIntensityConfig.Value};
            FLASHLIGHT_INTENSITY_HELMET = new[] {prolightIntensityHelmetConfig.Value, flashlightIntensityHelmetConfig.Value};
            FLASHLIGHT_INTENSITY_MIN = new[] {prolightIntensityMinConfig.Value, flashlightIntensityMinConfig.Value};
            FLASHLIGHT_RANGE = new[] {prolightRangeConfig.Value, flashlightRangeConfig.Value};
            FLASHLIGHT_RANGE_MIN = new[] {prolightRangeMinConfig.Value, flashlightRangeMinConfig.Value};
            FLASHLIGHT_SPOTANGLE = new[] {prolightSpotAngleConfig.Value, flashlightSpotAngleConfig.Value};
            FLASHLIGHT_SPOTANGLE_MIN = new[] {prolightSpotAngleMinConfig.Value, flashlightSpotAngleMinConfig.Value};
            FLASHLIGHT_THRESHOLD = new[] {prolightThresholdConfig.Value, flashlightThresholdConfig.Value};
            
            FLICKER_INTERVAL = flickerIntervalConfig.Value;
            FLICKER_CHANCE = flickerChanceConfig.Value;
            FLICKER_CHANCE_INSANITY = flickerChanceinsanityConfig.Value;
            
            
            harmony.PatchAll(typeof(FlashlightItemPatch));

            netcodeValidator = new NetcodeValidator(PluginInfo.PLUGIN_GUID);
            netcodeValidator.PatchAll();
            
            netcodeValidator.BindToPreExistingObjectByBehaviour<FlashlightRework, FlashlightItem>();
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
