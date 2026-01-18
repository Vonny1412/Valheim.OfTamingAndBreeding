using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OfTamingAndBreeding
{
    // required
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]

    // OTAB can post-register recipes via Wacky by accessing its SetRecipeData() method via Reflections
    [BepInDependency(ThirdParty.WackysDatabaseBridge.PluginGuid, BepInDependency.DependencyFlags.SoftDependency)]

    // Seasons mod can alter pregnancy durations on specific seasons
    // it seems that the Seasons mod is using prefix-patch on Procreation.Procreate with first priority
    // anyway... let it load before OTAB
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]

    // ensure client has this mod with correct version
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]

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

            ThirdParty.WackysDatabaseBridge.Register();
            ThirdParty.ThirdPartyManager.TryGetAssemblies();

        }

    }

}

