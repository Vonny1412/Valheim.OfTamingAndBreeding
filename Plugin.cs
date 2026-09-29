using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Utils;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Integrations.Mods;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Processing.Registry;
using System;
using System.IO;



// todo: cleanup



namespace OfTamingAndBreeding
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]
    [BepInDependency(CllCBridge.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)] // lifecycle-aware CLLC inheritance
    [BepInDependency(ValheimPlusCompatibility.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Vonny1412.HoldToCommand",     BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Patch)] // ensure client has this mod with correct version
    
    public sealed partial class Plugin : BaseUnityPlugin
    {

        private static readonly string[] toleratedMods = new string[] {
            "oldmankatan.mods.tamesfollow",
            "com.L3ca.Beyondthepen",
        };
        
        private static readonly string[] incompatibleMods = new string[] {
            "meldurson.valheim.AllTameable",
            "maxfoxgaming.procreationplus",
        };

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

        private bool CheckModsInChainloader()
        {
            foreach (var guid in toleratedMods)
            {
                if (Integrations.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogWarning($"Mod '{meta.Name}' may not be compatible with OTAB");
                }
            }
            var allOkay = true;
            foreach (var guid in incompatibleMods)
            {
                if (Integrations.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogFatal($"Mod '{meta.Name}' is not compatible with OTAB");
                    allOkay = false;
                }
            }
            return allOkay;
        }

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
            //ServerDataDir = Path.Combine(BepInEx.Paths.PluginPath, Plugin.ModGuid, "Data");
            ServerDataDir = Path.Combine(BepInEx.Paths.ConfigPath, Plugin.ModGuid);
            CacheDir = Path.Combine(BepInEx.Paths.CachePath, Plugin.ModGuid);

            Directory.CreateDirectory(ServerDataDir);
            Directory.CreateDirectory(CacheDir);

            if (CheckModsInChainloader() == false)
            {
                LogFatal($"Incompatible Mod(s) found - Abort loading");
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
            Network.NetworkSessionManager.OnSessionStarted += OnNetworkSessionStarted;
            Network.NetworkSessionManager.OnSessionReady += OnNetworkSessionReady;
            Network.NetworkSessionManager.OnSessionClosed += OnNetworkSessionClosed;
            Network.NetworkSessionManager.OnSessionError += OnNetworkSessionError;
        }

        private static void OnNetworkSessionStarted()
        {
            if (Network.NetworkSessionManager.IsServer())
            {
                if (Configs.DumpPrefabsToCache.Value == true)
                {
                    PrefabUtils.DumpPrefabs(Path.Combine(CacheDir, "prefabs"));
                }
            }

            Runtime.ZNetSceneContext.Block();
            OnSessionStarted();
        }

        private static void OnNetworkSessionReady()
        {
            OTABComponentTypeRegistry.AddComponentsToPrefabs();

            if (DataProcessingManager.IsDataLoaded)
            {
                foreach (var p in DataProcessingManager.DataProcessors)
                {
                    LogInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                }
                Patches.DataReadyPatches.Install();
            }
            else
            {
                LogInfo("No server sync detected (timeout). Running in vanilla mode.");
            }

            Runtime.ZNetSceneContext.Unblock();
            OnSessionReady();
        }

        private static void OnNetworkSessionClosed()
        {
            Patches.DataReadyPatches.Uninstall();

            OTABComponentTypeRegistry.RemoveComponentsFromPrefabs();

            OnSessionClosed();
            isAdmin = false;
        }

        private static void OnNetworkSessionError()
        {
            static void Logout()
            {
                Runtime.ZNetSceneContext.Clear();
                Game.instance.Logout(save: false, changeToStartScene: true);
            }
            if (UnifiedPopup.IsAvailable())
            {
                UnifiedPopup.Push(
                    new WarningPopup(
                        "Of Taming And Breeding",
                        "There was an error loading OTAB data. Please check your LogOutput.log for details.",
                        () => Logout(),
                        false
                    )
                );
            }
            else
            {
                Logout();
            }
        }

        private static bool? isAdmin = null;
        internal static bool IsAdmin()
        {
            if (isAdmin.HasValue)
            {
                return isAdmin.Value;
            }
            ZNet znet = ZNet.instance;
            if (znet == null)
            {
                return false;
            }
            var val = znet.LocalPlayerIsAdminOrHost();
            isAdmin = val;
            return val;
        }

        public static bool IsOTABMode()
        {
            return DataProcessingManager.IsDataLoaded;
        }
        
        public static void OnSessionStarted()
        {
        }

        public static void OnSessionReady()
        {
        }

        public static void OnSessionClosed()
        {
        }

    }

}