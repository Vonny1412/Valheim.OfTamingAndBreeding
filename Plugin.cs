using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Utils;
using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.OTABUtils;
using OfTamingAndBreeding.StaticContext;
using OfTamingAndBreeding.ThirdParty.Mods;
using System;
using System.IO;

namespace OfTamingAndBreeding
{
    // required
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]



    [BepInDependency(WackyDBBridge.PluginGUID,      BepInDependency.DependencyFlags.SoftDependency)] // OTAB can post-register recipes
    [BepInDependency(CllCBridge.PluginGUID,         BepInDependency.DependencyFlags.SoftDependency)] // lifecycle-aware CLLC inheritance
    [BepInDependency("shudnal.Seasons",             BepInDependency.DependencyFlags.SoftDependency)] // Seasons mod can alter pregnancy durations on specific seasons
    [BepInDependency("digitalroot.mods.GoldBars", BepInDependency.DependencyFlags.SoftDependency)] // maybe used for recipes
    [BepInDependency("Vonny1412.HoldToCommand", BepInDependency.DependencyFlags.SoftDependency)]

    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)] // ensure client has this mod with correct version

    public sealed partial class Plugin : BaseUnityPlugin
    {
        private static readonly string[] supportedMods = new string[] {
            CllCBridge.PluginGUID,
            WackyDBBridge.PluginGUID,
            "shudnal.Seasons",
            "digitalroot.mods.GoldBars",
            "",
        };
        
        private static readonly string[] toleratedMods = new string[] {
            "oldmankatan.mods.tamesfollow",
            "com.L3ca.Beyondthepen",
            "maxfoxgaming.procreationplus",
        };

        private static readonly string[] incompatibleMods = new string[] {
            "meldurson.valheim.AllTameable",
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

        internal static void LogServerWarning(object data)
        {
            if (Net.NetworkSessionManager.Instance.IsServer())
            {
                Instance.Logger.LogWarning(data);
            }
        }

        internal static void LogServerMessage(object data)
        {
            if (Net.NetworkSessionManager.Instance.IsServer())
            {
                Instance.Logger.LogMessage(data);
            }
        }

        internal static void LogServerInfo(object data)
        {
            if (Net.NetworkSessionManager.Instance.IsServer())
            {
                Instance.Logger.LogInfo(data);
            }
        }

        internal static void LogServerDebug(object data)
        {
            if (Net.NetworkSessionManager.Instance.IsServer())
            {
                Instance.Logger.LogDebug(data);
            }
        }

        private bool CheckModsInChainloader()
        {
            foreach (var guid in supportedMods)
            {
                if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogInfo($"Mod '{meta.Name}' is on board");
                }
            }
            foreach (var guid in toleratedMods)
            {
                if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    LogWarning($"Mod '{meta.Name}' may not be compatible with OTAB");
                }
            }
            var allOkay = true;
            foreach (var guid in incompatibleMods)
            {
                if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
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
            ServerDataDir = Path.Combine(BepInEx.Paths.PluginPath, Plugin.ModGuid, "Data");
            CacheDir = Path.Combine(BepInEx.Paths.CachePath, Plugin.ModGuid);

            if (CheckModsInChainloader() == false)
            {
                LogFatal($"Incompatible Mod(s) found - Abort loading");
                DisablePlugin();
                return;
            }

            try
            {
                Patches.AlwaysActivePatches.Install();
                // because this way we can see if all signatures are valid
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

            Net.RPCContext.RegisterRPCs();
            ThirdParty.ThirdPartyManager.RegisterBridges();

            Data.CacheManager.CreateInstance();
            Registry.PrefabRegistryManager.CreateInstance();
            Net.NetworkSessionManager.CreateInstance();
            Net.NetworkSessionManager.Instance.OnSessionStarted += OnNetworkSessionStarted;
            Net.NetworkSessionManager.Instance.OnSessionReady += OnNetworkSessionReady;
            Net.NetworkSessionManager.Instance.OnSessionClosed += OnNetworkSessionClosed;
        }

        private static void OnNetworkSessionStarted(Net.NetworkSessionManager netsess)
        {
            if (netsess.IsServer())
            {
                if (Configs.DumpPrefabsToCache.Value == true)
                {
                    PrefabUtils.DumpPrefabs(Path.Combine(CacheDir, "prefabs"));
                }
            }
            else
            {
                ZNetSceneContext.Block();
            }
            OnSessionStarted();
        }

        private static void OnNetworkSessionReady(Net.NetworkSessionManager netsess, bool dataLoaded)
        {

            // add trait components to all prefabs that are still missing these traits
            // added trait types will be registered and latter removed when session is closing
            AnimalAITrait.AddComponentToPrefabs(typeof(AnimalAI));
            BaseAITrait.AddComponentToPrefabs(typeof(BaseAI));
            CharacterTrait.AddComponentToPrefabs(typeof(Character), typeof(BaseAI));
            EggGrowTrait.AddComponentToPrefabs(typeof(EggGrow));
            GrowupTrait.AddComponentToPrefabs(typeof(Growup));
            ItemDropTrait.AddComponentToPrefabs(typeof(ItemDrop));
            MonsterAITrait.AddComponentToPrefabs(typeof(MonsterAI));
            ProcreationTrait.AddComponentToPrefabs(typeof(Procreation));
            TameableTrait.AddComponentToPrefabs(typeof(Tameable));

            if (dataLoaded)
            {
                Patches.DataReadyPatches.Install();
                if (netsess.IsServer())
                {
                    foreach (var p in Registry.PrefabRegistryManager.Instance.IterDataProcessors())
                    {
                        LogInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                    }
                }
                Features.LocalIdleAnimations.RemoveIdleEvents();
            }
            else
            {
                LogInfo("No server sync detected (timeout). Running in vanilla mode.");
            }

            if (!netsess.IsServer())
            {
                ZNetSceneContext.Unblock();
            }
            OnSessionReady(dataLoaded);
        }

        private static void OnNetworkSessionClosed(Net.NetworkSessionManager netsess, bool dataLoaded)
        {
            if (dataLoaded)
            {
                Patches.DataReadyPatches.Uninstall();
            }

            OTABComponentRegistry.RemoveComponentsFromPrefabs();

            OnSessionClosed(dataLoaded);
            isAdmin = false;
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

        public static bool IsServerDataLoaded()
        {
            return Registry.PrefabRegistryManager.Instance != null && Registry.PrefabRegistryManager.Instance.IsDataLoaded();
        }
        
        public static void OnSessionStarted()
        {
            // could be used as api
        }

        public static void OnSessionReady(bool dataLoaded)
        {
            // could be used as api
            // dataLoaded == true -> OTAB Mode
            // dataLoaded == false -> Vanilla Mode
        }

        public static void OnSessionClosed(bool dataLoaded)
        {
            // could be used as api
            // dataLoaded == true -> OTAB Mode
            // dataLoaded == false -> Vanilla Mode
        }

    }

}