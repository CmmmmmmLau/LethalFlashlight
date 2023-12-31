using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalFlashlight.Network;
using LethalFlashlight.Patches;
using UnityEngine;

namespace LethalFlashlight
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle MainAssetBundle;
        
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        
        private void Awake()
        {
            var stream  = Assembly.GetExecutingAssembly().GetManifestResourceStream("LethalFlashlight.Resources.flashlightasset");
            MainAssetBundle = AssetBundle.LoadFromStream(stream);
            
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types) {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods) {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0) {
                        method.Invoke(null, null);
                    }
                }
            }

            harmony.PatchAll(typeof(FlashlightNetworkHandler));
            harmony.PatchAll(typeof(FlashlightItemPatch));
            harmony.PatchAll(typeof(NetworkObjectPath));    
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
