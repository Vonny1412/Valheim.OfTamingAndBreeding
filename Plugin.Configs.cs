using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin : BaseUnityPlugin
    {

        public class Configs
        {
            public static ConfigEntry<bool> HoverShowLovePoints { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancyTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancy { get; private set; }
            public static ConfigEntry<bool> HoverShowFed { get; private set; }
            public static ConfigEntry<bool> HoverShowFedTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowFedTimerStarving { get; private set; }
            public static ConfigEntry<bool> HoverShowSeconds { get; private set; }
            public static ConfigEntry<bool> HoverUseIngameTime { get; private set; }
            public static ConfigEntry<string> HoverColorGood { get; private set; }
            public static ConfigEntry<string> HoverColorNormal { get; private set; }
            public static ConfigEntry<string> HoverColorBad { get; private set; }
            public static ConfigEntry<float> HoverProgressPrecision { get; private set; }

            // policy

            public static ConfigEntry<bool> RequireFoodDroppedByPlayer { get; private set; }
            public static ConfigEntry<bool> ShowEggGrowProgress { get; private set; }
            public static ConfigEntry<bool> ShowTamingProgress { get; private set; }
            public static ConfigEntry<bool> ShowOffspringGrowProgress { get; private set; }

            // cache

            public static ConfigEntry<bool> UseCache { get; private set; }
            public static ConfigEntry<string> CacheFileName { get; private set; }
            public static ConfigEntry<string> CacheFileCryptKey { get; private set; }
            public static ConfigEntry<string> CacheFileHash { get; private set; }

            public static void Initialize(ConfigFile Config)
            {
                ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes
                {
                    IsAdminOnly = true,
                };
                ConfigurationManagerAttributes isHidden = new ConfigurationManagerAttributes
                {
                    Browsable = false,
                };

                string section = "";

                section = "HoverText";

                HoverShowLovePoints = Config.Bind<bool>(section, "HoverShowLovePoints", true, new ConfigDescription(""));
                HoverShowPregnancy = Config.Bind<bool>(section, "HoverShowPregnancy", true, new ConfigDescription(""));
                HoverShowPregnancyTimer = Config.Bind<bool>(section, "HoverShowPregnancyTimer", true, new ConfigDescription(""));

                HoverShowFed = Config.Bind<bool>(section, "HoverShowFed", true, new ConfigDescription(""));
                HoverShowFedTimer = Config.Bind<bool>(section, "HoverShowFedTimer", true, new ConfigDescription(""));
                HoverShowFedTimerStarving = Config.Bind<bool>(section, "HoverShowFedTimerStarving", false, new ConfigDescription(""));

                HoverShowSeconds = Config.Bind<bool>(section, "HoverShowSeconds", false, new ConfigDescription(""));
                HoverUseIngameTime = Config.Bind<bool>(section, "HoverUseIngameTime", true, new ConfigDescription(""));

                HoverColorGood = Config.Bind<string>(section, "HoverColorGood", "#00FF00", new ConfigDescription(""));
                HoverColorNormal = Config.Bind<string>(section, "HoverColorNormal", "#999999", new ConfigDescription(""));
                HoverColorBad = Config.Bind<string>(section, "HoverColorBad", "#FFA500", new ConfigDescription(""));

                section = "Policy";

                RequireFoodDroppedByPlayer = Config.Bind<bool>(section, "RequireFoodDroppedByPlayer", true, new ConfigDescription("When disabled, animals can also be tamed and stimulated to breed using world/drop food. With Necks, this can lead to interesting ecosystems.", null, isAdminOnly));
                ShowEggGrowProgress = Config.Bind<bool>(section, "ShowEggGrowProgress", true, new ConfigDescription("", null, isAdminOnly));
                ShowTamingProgress = Config.Bind<bool>(section, "ShowTamingProgress", true, new ConfigDescription("", null, isAdminOnly));
                ShowOffspringGrowProgress = Config.Bind<bool>(section, "ShowOffspringGrowProgress", true, new ConfigDescription("", null, isAdminOnly));
                HoverProgressPrecision = Config.Bind<float>(section, "HoverProgressPrecision", 1, new ConfigDescription("", new AcceptableValueList<float>(1f, 0.5f, 0.25f, 0.2f, 0.1f, 0.05f, 0.025f, 0.02f, 0.01f, 0.005f, 0.0025f, 0.002f, 0.001f), isAdminOnly));

                section = "Cache";

                UseCache = Config.Bind<bool>(section, "UseCache", true, new ConfigDescription("", null, isAdminOnly));
                CacheFileName = Config.Bind<string>(section, "CacheFileName", "local-{world}-{seed}", new ConfigDescription("", null, isAdminOnly));
                CacheFileCryptKey = Config.Bind<string>(section, "CacheFileCryptKey", "MySecretKey", new ConfigDescription("", null, isHidden));
                CacheFileHash = Config.Bind<string>(section, "CacheFileHash", "", new ConfigDescription("", null, isAdminOnly));

            }
        }
    }
}
