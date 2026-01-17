using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

using OfTamingAndBreeding.Net;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance { get; private set; }
        internal static string ServerDataDir { get; private set; }
        internal static string CacheDir { get; private set; }

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
            RPCContext.RegisterRPCs();
        }

    }

}

