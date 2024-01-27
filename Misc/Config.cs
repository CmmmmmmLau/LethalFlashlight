using System.Runtime.Serialization;
using BepInEx.Configuration;
using HarmonyLib;

namespace LethalFlashlight.Misc;

[DataContract]
public class Config : SyncedInstance<Config> {
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
    
    [DataMember]public float[] FLASHLIGHT_INTENSITY;
    [DataMember]public float[] FLASHLIGHT_INTENSITY_HELMET;
    [DataMember]public float[] FLASHLIGHT_INTENSITY_MIN;
    [DataMember]public float[] FLASHLIGHT_RANGE;
    [DataMember]public float[] FLASHLIGHT_RANGE_MIN;
    [DataMember]public float[] FLASHLIGHT_SPOTANGLE;
    [DataMember]public float[] FLASHLIGHT_SPOTANGLE_MIN;
    [DataMember]public float[] FLASHLIGHT_THRESHOLD;
    
    [DataMember]public int FLICKER_INTERVAL;
    [DataMember]public float FLICKER_CHANCE;
    [DataMember]public float FLICKER_CHANCE_INSANITY;

    public Config(ConfigFile cfg) {
        InitInstance(this);
        
        this.flashlightIntensityConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Intensity", 397.9603f, "The default intensity of the Flashlight");
        this.flashlightIntensityHelmetConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Intensity Helmet", 833.2255f, "The default intensity of the Flashlight when used as a helmet light");
        this.flashlightIntensityMinConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Intensity Min", 0.1f, "The minimum intensity of the Flashlight(in percentage)");
        this.flashlightRangeConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Range", 17f, "The default range of the Flashlight");
        this.flashlightRangeMinConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Range Min", 17f, "The minimum range of the Flashlight");
        this.flashlightSpotAngleConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Spot Angle", 55.4f, "The default spot angle of the Flashlight");
        this.flashlightSpotAngleMinConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Spot Angle Min", 55.4f, "The minimum spot angle of the Flashlight");
        this.flashlightThresholdConfig = cfg.Bind<float>("Flashlight Tweak", "Flashlight Threshold", 0.6f, "The percentage of the Flashlight start losing power");
 
        this.prolightIntensityConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Intensity", 486.8536f, "The default intensity of the ProFlashlight");
        this.prolightIntensityHelmetConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Intensity Helmet", 486.8536f, "The default intensity of the ProFlashlight when used as a helmet light");
        this.prolightIntensityMinConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Threshold Min", 0.2f, "The minimum threshold of the ProFlashlight");
        this.prolightRangeConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Range", 55f, "The default range of the ProFlashlight");
        this.prolightRangeMinConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Range Min", 25f, "The minimum range of the ProFlashlight");
        this.prolightSpotAngleConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Spot Angle", 73f, "The default spot angle of the ProFlashlight");
        this.prolightSpotAngleMinConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Spot Angle Min", 50f, "The minimum spot angle of the ProFlashlight");
        this.prolightThresholdConfig = cfg.Bind<float>("ProFlashlight Tweak", "ProFlashlight Threshold", 0.5f, "The percentage of the ProFlashlight start losing power");

        this.flickerIntervalConfig = cfg.Bind<int>("Flicker", "Flicker Interval", 60, "The interval of the Flashlight Flicker in seconds");
        this.flickerChanceConfig = cfg.Bind<float>("Flicker", "Flicker Chance", 0.2f, "The chance of the Flashlight will flicking when the battery is low");
        this.flickerChanceinsanityConfig = cfg.Bind<float>("Flicker", "Flicker Chance Insanity", 0.3f, "The chance of the Flashlight will flicking when the player's insanity is high");

        
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
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "JoinLobby")]
    public static void InitializeLocalPlayer() {
        if (IsHost) {
            MessageManager.RegisterNamedMessageHandler("ModName_OnRequestConfigSync", OnRequestSync);
            Synced = true;

            return;
        }

        Synced = false;
        MessageManager.RegisterNamedMessageHandler("ModName_OnReceiveConfigSync", OnReceiveSync);
        RequestSync();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
    public static void PlayerLeave() {
        Config.RevertSync();
    }
}