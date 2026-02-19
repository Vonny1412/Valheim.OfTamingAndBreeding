using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn;
using Jotunn.Utils;
using OfTamingAndBreeding.Net;
using OfTamingAndBreeding.ThirdParty.Mods;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
        internal static void LogMessage(object data) => Instance.Logger.LogMessage(data);
        internal static void LogError(object data) => Instance.Logger.LogError(data);
        internal static void LogFatal(object data) => Instance.Logger.LogFatal(data);
        internal static void LogInfo(object data) => Instance.Logger.LogInfo(data);
        internal static void LogDebug(object data) => Instance.Logger.LogDebug(data);
        internal static void LogWarning(object data) => Instance.Logger.LogWarning(data);

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

            UnityEngine.Object.Destroy(this);
        }

        private void Awake()
        {
            Instance = this;
            ServerDataDir = Path.Combine(BepInEx.Paths.PluginPath, Plugin.ModGuid, "Data");
            CacheDir = Path.Combine(BepInEx.Paths.CachePath, Plugin.ModGuid);

            if (CheckModsInChainloader() == false)
            {
                Plugin.LogFatal($"Incompatible Mod(s) found - Abort loading");
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
                Plugin.LogFatal("Patch validation failed. This OTAB build is broken =(");
                Plugin.LogFatal(ex.ToString());
                DisablePlugin();
                throw;
            }

            Configs.Initialize(Config);
            Net.RPCContext.RegisterRPCs();
            ThirdParty.ThirdPartyManager.RegisterBridges();

            Data.DataOrchestrator.OnDataLoaded(() => {
                ZNetScene.instance?.UnblockObjectsCreation();
                Patches.DataReadyPatches.Install();
                _IsOTABReady = true;

                if (ZNet.instance.IsServer())
                {
                    foreach (var p in Data.DataOrchestrator.IterDataProcessors())
                    {
                        Plugin.LogInfo($"Loaded {p.GetLoadedDataCount()} {p.ModelTypeName} entries");
                    }
                }
            });
            Data.DataOrchestrator.OnDataReset(() => {
                Patches.DataReadyPatches.Uninstall();
                _IsOTABReady = false;
            });

        }

        // network stuff
        // todo: maybe create own class for this?

        public static bool _IsOTABReady { get; private set; } = false;
        private static Coroutine clientTimeoutRoutine;

        internal static void InitSession()
        {
            var zn = ZNet.instance;
            var isLocal = zn.IsLocalInstance();
            if (zn.IsServer())
            {
                ZNetSceneExtensions.ObjectsContext.blockObjectsCreation = false;
                RPCContext.InitServerSession(isLocal);
            }
            else
            {
                ZNetSceneExtensions.ObjectsContext.blockObjectsCreation = true;
                RPCContext.InitClientSession(isLocal);
            }
        }

        internal static void RequestHandshakeWithServer()
        {
            // only called for clients
            RPCContext.RequestHandshakeWithServer();
            StartClientTimeout(5f);
        }

        internal static void CloseSession()
        {
            // called for client and server

            Data.DataOrchestrator.ResetData();
            RPCContext.DestroySession();

            ZNetSceneExtensions.ObjectsContext.Clear();
            CancelClientTimeout();
        }

        public static void StartClientTimeout(float seconds)
        {
            if (clientTimeoutRoutine == null)
            {
                clientTimeoutRoutine = Instance.StartCoroutine(RunClientTimeout(seconds));
            }
        }

        public static void CancelClientTimeout()
        {
            if (clientTimeoutRoutine != null)
            {
                Instance.StopCoroutine(clientTimeoutRoutine);
                clientTimeoutRoutine = null;
            }
        }

        private static System.Collections.IEnumerator RunClientTimeout(float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds)
            {
                if (_IsOTABReady)
                {
                    yield break;
                }
                yield return null;
            }
            Plugin.LogInfo("No server sync detected (timeout). Running in vanilla mode.");
            ZNetScene.instance?.UnblockObjectsCreation();
            _IsOTABReady = false;
        }

    }

}

