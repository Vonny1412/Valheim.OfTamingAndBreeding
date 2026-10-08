using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Utils;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Integrations.Mods;
using System;
using System.IO;

namespace OfTamingAndBreeding
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]

    [BepInDependency(CllCBridge.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ValheimPlusCompatibility.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CoreWoodExtrasCompatibility.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Vonny1412.HoldToCommand",     BepInDependency.DependencyFlags.SoftDependency)]

    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Patch)] // ensure client has this mod with correct version

    public sealed partial class Plugin : BaseUnityPlugin
    {

        internal static Plugin Instance { get; private set; }
        public static string ServerDataDir { get; private set; }
        public static string CacheDir { get; private set; }

        // this way we can keep track where each loglevel is beeing used
        internal static void LogFatal(object data) => Instance.Logger.LogFatal(data);
        internal static void LogError(object data) => Instance.Logger.LogError(data);
        internal static void LogWarning(object data) => Instance.Logger.LogWarning(data);
        internal static void LogMessage(object data) => Instance.Logger.LogMessage(data);
        internal static void LogInfo(object data) => Instance.Logger.LogInfo(data);
        internal static void LogDebug(object data) => Instance.Logger.LogDebug(data);

        private void DisablePlugin()
        {
            Patches.AlwaysActivePatches.Uninstall();
            Patches.DataReadyPatches.Uninstall();

            Chainloader.PluginInfos.Remove(Plugin.ModGuid);

            enabled = false;
            Instance = null;

            Destroy(this);
        }

        private void Awake()
        {
            Instance = this;
            ServerDataDir = Path.Combine(BepInEx.Paths.ConfigPath, Plugin.ModGuid);
            CacheDir = Path.Combine(BepInEx.Paths.CachePath, Plugin.ModGuid);

            string[] toleratedMods = new string[] {
                "oldmankatan.mods.tamesfollow",
            };

            string[] incompatibleMods = new string[] {
                "meldurson.valheim.AllTameable",
                "maxfoxgaming.procreationplus",
                "com.L3ca.Beyondthepen",
            };

            foreach (var guid in toleratedMods)
            {
                if (Integrations.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogWarning($"Mod '{meta.Name}' may not be compatible with OTAB");
                }
            }
            var incompatibleModFound = false;
            foreach (var guid in incompatibleMods)
            {
                if (Integrations.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogFatal($"Mod '{meta.Name}' is not compatible with OTAB");
                    incompatibleModFound = true;
                }
            }
            if (incompatibleModFound)
            {
                DisablePlugin();
                return;
            }

            try
            {
                Patches.AlwaysActivePatches.Install();
                // this way we can see if all signatures are valid
                Patches.DataReadyPatches.Install();
                Patches.DataReadyPatches.Uninstall();
            }
            catch (Exception ex)
            {
                LogFatal("Patch validation failed. This OTAB build is broken.");
                LogFatal(ex.ToString());
                DisablePlugin();
                throw;
            }

            Configs.Initialize(Config);

            Directory.CreateDirectory(ServerDataDir);
            Directory.CreateDirectory(CacheDir);

            // we only allow real creatures - do not touch the stone!
            BaseAITrait.RegisterType(typeof(BaseAI), typeof(Character));
            AnimalAITrait.RegisterType(typeof(BaseAI), typeof(Character), typeof(AnimalAI));
            //MonsterAITrait.RegisterType(typeof(BaseAI), typeof(MonsterAI));
            CharacterTrait.RegisterType(typeof(BaseAI), typeof(Character));
            GrowupTrait.RegisterType(typeof(BaseAI), typeof(Character), typeof(Growup));
            TameableTrait.RegisterType(typeof(BaseAI), typeof(Character), typeof(Tameable));
            ProcreationTrait.RegisterType(typeof(BaseAI), typeof(Character), typeof(Procreation));
            PetTrait.RegisterType(typeof(BaseAI), typeof(Character), typeof(Pet));

            ItemDropTrait.RegisterType(typeof(ItemDrop));
            EggGrowTrait.RegisterType(typeof(ItemDrop), typeof(EggGrow));

            // Requiring itself prevents automatic addition, but still allows automatic removal.
            ScaledCreature.RegisterType(typeof(ScaledCreature));
            ScaledItem.RegisterType(typeof(ScaledItem));
            AnimationClipOverlay.RegisterType(typeof(AnimationClipOverlay));
            AttachedSprite.RegisterType(typeof(AttachedSprite));

            Integrations.ThirdPartyManager.RegisterBridges();
            Network.NetworkSessionManager.RegisterRPCs();

            /*
            
            LogMessage($"z_fedDurationFactor: {ZDOVars.z_fedDurationFactor}");
            LogMessage($"z_partnerPrefab: {ZDOVars.z_partnerPrefab}");
            LogMessage($"z_confined: {ZDOVars.z_confined}");
            LogMessage($"z_droppedByAnyPlayer: {ZDOVars.z_droppedByAnyPlayer}");
            LogMessage($"z_CLLC_Infusion: {ZDOVars.z_CLLC_Infusion}");
            LogMessage($"z_CLLC_Effect: {ZDOVars.z_CLLC_Effect}");
            LogMessage($"z_growTimeLeft: {ZDOVars.z_growTimeLeft}");

            [Message:Of Taming and Breeding] z_fedDurationFactor: 1191338377
            [Message:Of Taming and Breeding] z_partnerPrefab: 479200733
            [Message:Of Taming and Breeding] z_confined: -2083973897
            [Message:Of Taming and Breeding] z_droppedByAnyPlayer: 770701559
            [Message:Of Taming and Breeding] z_CLLC_Infusion: -1369819759
            [Message:Of Taming and Breeding] z_CLLC_Effect: -776129839
            [Message:Of Taming and Breeding] z_growTimeLeft: 1079401883

            */
        }

    }

}