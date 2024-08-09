using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ScavBirthdayParty
{
    [BepInPlugin("com.noodles.scavbirthdayparty", "Scav Birthday Party", "1.1")]
    public class ScavBirthdayParty : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        public static ConfigEntry<bool> headshotOnly { get; set; }
        public static ConfigEntry<bool> enableHooray { get; set; }
        public static ConfigEntry<bool> enableConfetti { get; set; }
        public static ConfigEntry<int> confettiAmount { get; set; }
        public static ConfigEntry<int> confettiForce { get; set; }
        public static ConfigEntry<int> confettiChance { get; set; }
        public static ConfigEntry<float> hoorayVolume { get; set; }

        public static GameObject confettiEffectPrefab;
        public static GameObject hooraySoundPrefab;
        private Material confettiMaterial;
        private Shader confettiShader;

        void Awake()
        {
            logger = Logger;

            headshotOnly = Config.Bind("1. Settings", "Only On Headshots", true);
            enableHooray = Config.Bind("1. Settings", "Enable Hooray Sound", true);
            enableConfetti = Config.Bind("1. Settings", "Enable Confetti Effect", true);
            confettiAmount = Config.Bind("2. Confetti", "Confetti Amount", 256, "Max 1024");
            confettiForce = Config.Bind("2. Confetti", "Confetti Force", 2);
            confettiChance = Config.Bind("2. Confetti", "Confetti Chance", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
            hoorayVolume = Config.Bind("3. Hooray", "Hooray Volume", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0.0f, 1.0f)));

            confettiAmount.SettingChanged += (sender, args) =>
            {
                ConfigEntryBase configEntryBase = ((SettingChangedEventArgs)args).ChangedSetting;
                var prefabParticleSystem = confettiEffectPrefab.GetComponent<ParticleSystem>().main;
                prefabParticleSystem.maxParticles = (int)configEntryBase.BoxedValue;
            };

            confettiForce.SettingChanged += (sender, args) =>
            {
                ConfigEntryBase configEntryBase = ((SettingChangedEventArgs)args).ChangedSetting;
                var prefabParticleSystem = confettiEffectPrefab.GetComponent<ParticleSystem>().main;
                prefabParticleSystem.startSpeed = (int)configEntryBase.BoxedValue;
            };

            hoorayVolume.SettingChanged += (sender, args) =>
            {
                ConfigEntryBase configEntryBase = ((SettingChangedEventArgs)args).ChangedSetting;
                var hoorayAudioSourcePrefab = hooraySoundPrefab.GetComponent<AudioSource>();
                hoorayAudioSourcePrefab.volume = (float)configEntryBase.BoxedValue;
            };

            new OnBeenKilledByAggressorPatch().Enable();

            LoadAssets();
            logger.LogInfo("Loaded Scav Birthday Party");
        }

        void Start()
        {
            confettiMaterial.shader = confettiShader;
        }

        private void LoadAssets()
        {
            string bundleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ScavBirthdayParty\\";
            if (Directory.Exists(bundleDirectory))
            {
                AssetBundle scavBirthdayAssetBundle = AssetBundle.LoadFromFile(bundleDirectory + "\\scavbirthday");
                Object[] loadedAssets = scavBirthdayAssetBundle.LoadAllAssets();
                logger.LogInfo($"Loaded {loadedAssets.Length} assets from assetbundle!");

                confettiEffectPrefab = (GameObject)loadedAssets[1];
                hooraySoundPrefab = (GameObject)loadedAssets[2];
                confettiMaterial = (Material)loadedAssets[3];
                confettiShader = (Shader)loadedAssets[0];
            }
            else
            {
                logger.LogError($"Failed to load assetbundle {bundleDirectory}!");
            }
        }
    }
}
