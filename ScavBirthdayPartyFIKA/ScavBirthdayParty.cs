using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ScavBirthdayParty
{
    [BepInPlugin("com.noodles.scavbirthdaypartyfika", "Scav Birthday Party FIKA", "1.1")]
    [BepInDependency("com.fika.core")]
    public class ScavBirthdayParty : BaseUnityPlugin
    {
        public static ManualLogSource logger { get; private set; }
        public static NetDataWriter writer { get; private set; }

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
            writer = new NetDataWriter();

            FikaEventDispatcher.SubscribeEvent<FikaClientCreatedEvent>(OnClientCreated);
            FikaEventDispatcher.SubscribeEvent<FikaServerCreatedEvent>(OnServerCreated);

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

        private void OnClientCreated(FikaClientCreatedEvent @event)
        {
            @event.Client.packetProcessor.SubscribeNetSerializable<BirthdayPartyPacket>(OnReceievePacketClient);
        }

        private void OnServerCreated(FikaServerCreatedEvent @event)
        {
            @event.Server.packetProcessor.SubscribeNetSerializable<BirthdayPartyPacket>(OnReceivePacketServer);
        }

        private void OnReceievePacketClient(BirthdayPartyPacket packet)
        {
            if (!Singleton<FikaClient>.Instantiated) { return; }
            OnBeenKilledByAggressorPatch.CreateScavBirthdayEffect(packet.position);
        }

        private void OnReceivePacketServer(BirthdayPartyPacket packet)
        {
            if (!Singleton<FikaServer>.Instantiated) { return; }
            OnBeenKilledByAggressorPatch.CreateScavBirthdayEffect(packet.position);
            Singleton<FikaServer>.Instance.SendDataToAll(writer, ref packet, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }
}
