using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using OfTamingAndBreeding.ThirdParty.Mods;
namespace OfTamingAndBreeding
{
    // required
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]

    [BepInDependency(WackyDBBridge.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)] // OTAB can post-register recipes
    [BepInDependency(CllCBridge.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)] // Seasons mod can alter pregnancy durations on specific seasons

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)] // ensure client has this mod with correct version

    public sealed partial class Plugin : BaseUnityPlugin
    {
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

        internal const string DefaultWorldDirectory = "__default";
        internal const string OriginalDataDirectory = "__original";

        private void Awake()
        {
            Instance = this;
            ServerDataDir = Path.Combine(BepInEx.Paths.PluginPath, Plugin.ModGuid, "Data");
            CacheDir = Path.Combine(BepInEx.Paths.CachePath, Plugin.ModGuid);

            Configs.Initialize(Config);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Net.RPCContext.RegisterRPCs();

            // make sure to add BepInDependency too!
            ThirdParty.ThirdPartyManager.RegisterBridges();

        }

    }

}

