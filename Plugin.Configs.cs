using BepInEx;
using BepInEx.Configuration;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Internals.API.UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using static Character;
using static Player;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {

        public class Configs
        {
            public static ConfigEntry<bool> HoverShowSeconds { get; private set; }
            public static ConfigEntry<bool> HoverUseIngameTime { get; private set; }
            public static ConfigEntry<string> HoverColorGood { get; private set; }
            public static ConfigEntry<string> HoverColorNormal { get; private set; }
            public static ConfigEntry<string> HoverColorBad { get; private set; }

            // policy

            public static ConfigEntry<bool> HoverShowLovePoints { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancy { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancyTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowFedTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowHungryTimer { get; private set; }

            public static ConfigEntry<bool> RequireFoodDroppedByPlayer { get; private set; }
            public static ConfigEntry<bool> ShowEggGrowProgress { get; private set; }
            public static ConfigEntry<bool> ShowTamingProgress { get; private set; }
            public static ConfigEntry<bool> ShowOffspringGrowProgress { get; private set; }
            public static ConfigEntry<float> HoverProgressPrecision { get; private set; }
            public static ConfigEntry<bool> UseBetterSearchForFood { get; private set; }
            public static ConfigEntry<float> TamingSlowdownPerStar { get; private set; }
            public static ConfigEntry<bool> MourningResetsLovePoints { get; private set; }

            // cache

            public static ConfigEntry<bool> WriteClientCacheFile { get; private set; }
            public static ConfigEntry<bool> WriteServerCacheDebugFiles { get; private set; }
            public static ConfigEntry<string> CacheFileName { get; private set; }
            public static ConfigEntry<string> CacheFileCryptKey { get; private set; }

            // CLLC

            public static ConfigEntry<float> CLLC_Infusion_WeightDirectParent { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightCurrentPartner { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightAnyNearby { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightRandomNew { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightNone { get; private set; }

            public static ConfigEntry<float> CLLC_Infusion_SearchRange { get; private set; }

            public static ConfigEntry<float> CLLC_Effect_WeightDirectParent { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightCurrentPartner { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightAnyNearby { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightRandomNew { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightNone { get; private set; }

            public static ConfigEntry<float> CLLC_Effect_SearchRange { get; private set; }

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

                HoverShowSeconds = Config.Bind<bool>(section, "HoverShowSeconds", false, new ConfigDescription("Show seconds in hover timers (otherwise only days/hours/minutes)."));
                HoverUseIngameTime = Config.Bind<bool>(section, "HoverUseIngameTime", true, new ConfigDescription("Format timers using Valheim in-game time (day length) instead of real time."));

                HoverColorGood = Config.Bind<string>(section, "HoverColorGood", "#00FF00", new ConfigDescription("Color used for positive/healthy hover states (hex color/color name)."));
                HoverColorNormal = Config.Bind<string>(section, "HoverColorNormal", "#EEEEEE", new ConfigDescription("Color used for neutral hover states (hex color/color name)."));
                HoverColorBad = Config.Bind<string>(section, "HoverColorBad", "#FFA500", new ConfigDescription("Color used for negative/critical hover states (hex color/color name)."));

                section = "Policy";

                HoverShowLovePoints = Config.Bind<bool>(section, "HoverShowLovePoints", true, new ConfigDescription("Allow showing love points in creature hover text (server policy).", null, isAdminOnly));
                HoverShowPregnancy = Config.Bind<bool>(section, "HoverShowPregnancy", true, new ConfigDescription("Allow showing pregnancy status in creature hover text (server policy).", null, isAdminOnly));
                HoverShowPregnancyTimer = Config.Bind<bool>(section, "HoverShowPregnancyTimer", true, new ConfigDescription("Allow showing pregnancy timer in hover text (server policy).", null, isAdminOnly));
                HoverShowFedTimer = Config.Bind<bool>(section, "HoverShowFedTimer", true, new ConfigDescription("Allow showing fed/starving timer in hover text (server policy).", null, isAdminOnly));
                HoverShowHungryTimer = Config.Bind<bool>(section, "HoverShowHungryTimer", true, new ConfigDescription("If enabled, also show the timer while hungry (negative time).", null, isAdminOnly));
                HoverProgressPrecision = Config.Bind<float>(section, "HoverProgressPrecision", 1f, new ConfigDescription("Controls hover progress rounding (lower = more precise). Example: 1 = 1%, 0.1 = 0.1%.", new AcceptableValueList<float>(1f, 0.5f, 0.25f, 0.2f, 0.1f, 0.05f, 0.025f, 0.02f, 0.01f, 0.005f, 0.0025f, 0.002f, 0.001f), isAdminOnly));

                ShowEggGrowProgress = Config.Bind<bool>(section, "ShowEggGrowProgress", true, new ConfigDescription("Shows egg hatching progress as a percentage next to the egg's name in the HUD (visible from distance).", null, isAdminOnly));
                ShowTamingProgress = Config.Bind<bool>(section, "ShowTamingProgress", true, new ConfigDescription("Shows taming progress as a percentage next to the creature's name in the HUD (visible from distance).", null, isAdminOnly));
                ShowOffspringGrowProgress = Config.Bind<bool>(section, "ShowOffspringGrowProgress", true, new ConfigDescription("Shows offspring growth progress as a percentage next to the creature's name in the HUD (visible from distance).", null, isAdminOnly));

                RequireFoodDroppedByPlayer = Config.Bind<bool>(section, "RequireFoodDroppedByPlayer", true, new ConfigDescription("When disabled, animals will also be tamed and stimulated to breed using food found in the world (not dropped by players).", null, isAdminOnly));
                UseBetterSearchForFood = Config.Bind<bool>(section, "UseBetterSearchForFood", true, new ConfigDescription("Uses a weighted food search instead of always picking the nearest item, resulting in more natural and less robotic animal behavior.", null, isAdminOnly));
                TamingSlowdownPerStar = Config.Bind<float>(section, "TamingSlowdownPerStar", 1f, new ConfigDescription("Slows down taming progress per star by reducing how much progress is applied each update.\n" +
                    "This does not change the base taming time itself — it only affects how fast taming progresses.\nA value of 0 disables this feature.\n" +
                    "Formula: progress /= (1 + stars × value)\n" +
                    "Example (base time = 100s, value = 1.0): 0★ = 100s, 1★ = 200s, 2★ = 300s", null, isAdminOnly));

                MourningResetsLovePoints = Config.Bind<bool>(section, "MourningResetsLovePoints", true, new ConfigDescription("If enabled, love points are reset when a creature loses its partner after the mourning period.", null, isAdminOnly));

                section = "Cache";

                WriteClientCacheFile = Config.Bind<bool>(section, "WriteClientCacheFile", true, new ConfigDescription("Allows clients to write and use a local cache file. Not required for singleplayer worlds.", null, isAdminOnly));
                WriteServerCacheDebugFiles = Config.Bind<bool>(section, "WriteServerCacheDebugFiles", true, new ConfigDescription("Writes cleaned-up YAML data and an unencrypted cache file on the server for debugging purposes.", null, isAdminOnly));
                CacheFileName = Config.Bind<string>(section, "CacheFileName", "local-{world}-{seed}", new ConfigDescription("Template for the cache file name. Supports placeholders like {world} and {seed}. The final name is resolved by the server.", null, isAdminOnly));
                CacheFileCryptKey = Config.Bind<string>(section, "CacheFileCryptKey", "", new ConfigDescription("Key used to obfuscate the cache file contents. This is NOT secure encryption. Do NOT use real passwords or personal secrets.", null, isHidden));

                section = "CLLC";

                CLLC_Infusion_WeightDirectParent = Config.Bind<float>(section, "CLLC_Infusion_WeightDirectParent", 60, new ConfigDescription("", null, isAdminOnly));
                CLLC_Infusion_WeightCurrentPartner = Config.Bind<float>(section, "CLLC_Infusion_WeightCurrentPartner", 20, new ConfigDescription("", null, isAdminOnly));
                CLLC_Infusion_WeightAnyNearby = Config.Bind<float>(section, "CLLC_Infusion_WeightAnyNearby", 10, new ConfigDescription("", null, isAdminOnly));
                CLLC_Infusion_WeightRandomNew = Config.Bind<float>(section, "CLLC_Infusion_WeightRandomNew", 5, new ConfigDescription("", null, isAdminOnly));
                CLLC_Infusion_WeightNone = Config.Bind<float>(section, "CLLC_Infusion_WeightNone", 5, new ConfigDescription("", null, isAdminOnly));

                CLLC_Infusion_SearchRange = Config.Bind<float>(section, "CLLC_Infusion_SearchRange", 10, new ConfigDescription("", null, isAdminOnly));

                CLLC_Effect_WeightDirectParent = Config.Bind<float>(section, "CLLC_Effect_WeightDirectParent", 60, new ConfigDescription("", null, isAdminOnly));
                CLLC_Effect_WeightCurrentPartner = Config.Bind<float>(section, "CLLC_Effect_WeightCurrentPartner", 20, new ConfigDescription("", null, isAdminOnly));
                CLLC_Effect_WeightAnyNearby = Config.Bind<float>(section, "CLLC_Effect_WeightAnyNearby", 10, new ConfigDescription("", null, isAdminOnly));
                CLLC_Effect_WeightRandomNew = Config.Bind<float>(section, "CLLC_Effect_WeightRandomNew", 5, new ConfigDescription("", null, isAdminOnly));
                CLLC_Effect_WeightNone = Config.Bind<float>(section, "CLLC_Effect_WeightNone", 5, new ConfigDescription("", null, isAdminOnly));

                CLLC_Effect_SearchRange = Config.Bind<float>(section, "CLLC_Effect_SearchRange", 10, new ConfigDescription("", null, isAdminOnly));



            }
        }
    }
}
