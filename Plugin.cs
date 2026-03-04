using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Utils;
using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.StaticContext;
using OfTamingAndBreeding.ThirdParty.Mods;
using System;
using System.IO;

namespace OfTamingAndBreeding
{
    // required
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]

    [BepInDependency(WackyDBBridge.PluginGUID,  BepInDependency.DependencyFlags.SoftDependency)] // OTAB can post-register recipes
    [BepInDependency(CllCBridge.PluginGUID,     BepInDependency.DependencyFlags.SoftDependency)] // lifecycle-aware CLLC inheritance
    [BepInDependency("shudnal.Seasons",         BepInDependency.DependencyFlags.SoftDependency)] // Seasons mod can alter pregnancy durations on specific seasons

    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)] // ensure client has this mod with correct version

    public sealed partial class Plugin : BaseUnityPlugin
    {
        private static readonly string[] supportedMods = new string[] {
            CllCBridge.PluginGUID,
            WackyDBBridge.PluginGUID,
            "shudnal.Seasons",
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

        internal static void LogServerMessage(object data) {
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
                    Plugin.LogInfo($"Mod '{meta.Name}' is on board");
                }
            }
            foreach (var guid in toleratedMods)
            {
                if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    Plugin.LogWarning($"Mod '{meta.Name}' may not be compatible with OTAB");
                }
            }
            var allOkay = true;
            foreach (var guid in incompatibleMods)
            {
                if (ThirdParty.ThirdPartyManager.TryGetPluginMetadata(guid, out var meta))
                {
                    Plugin.LogFatal($"Mod '{meta.Name}' is not compatible with OTAB");
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
                LogFatal(@"Patch validation failed. This OTAB build is broken.");
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

            Net.NetworkSessionManager.Instance.OnReady((dataLoaded) => {
                if (dataLoaded)
                {
                    Patches.DataReadyPatches.Install();
                    foreach (var p in Registry.PrefabRegistryManager.Instance.IterDataProcessors())
                    {
                        LogServerInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                    }
                }
                else
                {
                    LogInfo("No server sync detected (timeout). Running in vanilla mode.");
                }

                // unblock objects creation
                ZNetSceneContext.blockObjectsCreation = false;
                while (ZNetSceneContext.pending.Count > 0)
                {
                    var (near, distant) = ZNetSceneContext.pending.Dequeue();
                    ZNetScene.instance.CreateObjects(near, distant);
                }

                TameableTrait.AddComponentToPrefabs(typeof(Tameable));
                EggGrowTrait.AddComponentToPrefabs(typeof(EggGrow));
                GrowupTrait.AddComponentToPrefabs(typeof(Growup));
                CharacterTrait.AddComponentToPrefabs(typeof(Character), typeof(BaseAI));
                ProcreationTrait.AddComponentToPrefabs(typeof(Procreation));
                BaseAITrait.AddComponentToPrefabs(typeof(BaseAI));
                ItemDropTrait.AddComponentToPrefabs(typeof(ItemDrop));
                MonsterAITrait.AddComponentToPrefabs(typeof(MonsterAI));

                OnSessionReady(dataLoaded);
            });

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                if (dataLoaded)
                {
                    Patches.DataReadyPatches.Uninstall();
                }

                OTABComponentRegistry.RemoveComponentsFromPrefabs();

                OnSessionClosed(dataLoaded);
            });

        }

        public static bool IsServerDataLoaded()
        {
            return Net.NetworkSessionManager.Instance != null && Net.NetworkSessionManager.Instance.IsServerDataLoaded();
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

