using BepInEx.Configuration;
using Jotunn.Extensions;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        public class Configs
        {

            private const string Section_UI_Hovertext = "UI - Hovertext";

            public static ConfigEntry<bool> HoverShowConsumeItems { get; private set; }
            public static ConfigEntry<bool> HoverShowLovePoints { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancyTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowFedTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowStarvingTimer { get; private set; }

            public static ConfigEntry<bool> HoverShowSeconds { get; private set; }
            public static ConfigEntry<bool> HoverUseIngameTime { get; private set; }

            public static ConfigEntry<string> HoverColorGood { get; private set; }
            public static ConfigEntry<string> HoverColorNormal { get; private set; }
            public static ConfigEntry<string> HoverColorBad { get; private set; }


            private const string Section_UI_Hud = "UI - Hud";

            public static ConfigEntry<bool> HudShowEggGrowProgress { get; private set; }
            public static ConfigEntry<bool> HudShowTamingProgress { get; private set; }
            public static ConfigEntry<bool> HudShowOffspringGrowProgress { get; private set; }

            public static ConfigEntry<float> HudProgressPrecision { get; private set; }


            private const string Section_Server_Data = "Server - Data";

            public static ConfigEntry<string> DefaultWorldDirectory { get; private set; }


            private const string Section_Server_Cache = "Server - Cache";

            public static ConfigEntry<bool> WriteClientCacheFile { get; private set; }
            public static ConfigEntry<bool> WriteServerCacheFiles { get; private set; }
            public static ConfigEntry<string> CacheFileName { get; private set; }
            public static ConfigEntry<string> CacheFileCryptKey { get; private set; }


            private const string Section_Server_Gameplay = "Server - Gameplay";

            public static ConfigEntry<float> DefaultStarvingGraceFactor { get; private set; }

            public static ConfigEntry<float> GlobalPregnancyDurationFactor { get; private set; }
            public static ConfigEntry<float> GlobalFedDurationFactor { get; private set; }
            public static ConfigEntry<float> GlobalTamingTimeFactor { get; private set; }
            public static ConfigEntry<float> GlobalGrowTimeFactor { get; private set; }

            public static ConfigEntry<bool> RequireFoodDroppedByPlayer { get; private set; }
            public static ConfigEntry<bool> UseBetterSearchForFood { get; private set; }
            public static ConfigEntry<float> TamingSlowdownPerStar { get; private set; }


            private const string Section_Server_CLLC = "Server - Creature Level & Loot Control";

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
                string section = "";


                section = Section_UI_Hovertext;

                HoverShowConsumeItems = Config.BindConfigInOrder<bool>(section, "ShowConsumeItems", true, "Allow showing consumeable items in creature hover text.", synced: false);
                HoverShowLovePoints = Config.BindConfigInOrder<bool>(section, "ShowLovePoints", true, "Allow showing love points in creature hover text.", synced: false);
                HoverShowPregnancyTimer = Config.BindConfigInOrder<bool>(section, "ShowPregnancyTimer", true, "Allow showing pregnancy timer in hover text.", synced: false);
                HoverShowFedTimer = Config.BindConfigInOrder<bool>(section, "ShowFedTimer", true, "Allow showing fed timer in hover text.", synced: false);
                HoverShowStarvingTimer = Config.BindConfigInOrder<bool>(section, "ShowStarvingTimer", true, "Displays a timer indicating when the creature will start starving in the hover text.\r\nThe timer is only shown if OTAB is also installed on the server.", synced: false);

                HoverShowSeconds = Config.BindConfigInOrder<bool>(section, "ShowSeconds", false, "Show seconds in hover timers (otherwise only days/hours/minutes).", synced: false);
                HoverUseIngameTime = Config.BindConfigInOrder<bool>(section, "UseIngameTime", true, "Format timers using Valheim in-game time (day length) instead of real time.", synced: false);

                HoverColorGood = Config.BindConfigInOrder<string>(section, "ColorGood", "#00FF00", "Color used for positive/healthy hover states (hex color/color name).", synced: false);
                HoverColorNormal = Config.BindConfigInOrder<string>(section, "ColorNormal", "#EEEEEE", "Color used for neutral hover states (hex color/color name).", synced: false);
                HoverColorBad = Config.BindConfigInOrder<string>(section, "ColorBad", "#FFA500", "Color used for negative/critical hover states (hex color/color name).", synced: false);


                section = Section_UI_Hud;

                HudShowEggGrowProgress = Config.BindConfigInOrder<bool>(section, "ShowEggGrowProgress", true, "Shows egg hatching progress as a percentage next to the egg's name in the HUD (visible from distance).", synced: false);
                HudShowTamingProgress = Config.BindConfigInOrder<bool>(section, "ShowTamingProgress", true, "Shows taming progress as a percentage next to the creature's name in the HUD (visible from distance).", synced: false);
                HudShowOffspringGrowProgress = Config.BindConfigInOrder<bool>(section, "ShowOffspringGrowProgress", true, "Shows offspring growth progress as a percentage next to the creature's name in the HUD (visible from distance).", synced: false);

                HudProgressPrecision = Config.BindConfigInOrder<float>(section, "ProgressPrecision", 1f, "Controls hover progress rounding (lower = more precise). Example: 1 = 1%, 0.1 = 0.1%.", acceptableValues: new AcceptableValueList<float>(1f, 0.5f, 0.25f, 0.2f, 0.1f, 0.05f, 0.025f, 0.02f, 0.01f, 0.005f, 0.0025f, 0.002f, 0.001f), synced: false);


                section = Section_Server_Data;

                DefaultWorldDirectory = Config.BindConfigInOrder<string>(section, "DefaultWorldDirectory", "default", "Fallback world data folder name used when no folder matches the current world name. Read on world/server start.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });


                section = Section_Server_Cache;

                WriteClientCacheFile = Config.BindConfigInOrder<bool>(section, "WriteClientCacheFile", true, "Allow clients to create and use a local OTAB cache file. This setting is read on world/server start and is not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                WriteServerCacheFiles = Config.BindConfigInOrder<bool>(section, "WriteServerCacheFiles", true, "Write debug output on the server (cleaned YAML and an unencrypted cache file). This setting is read on world/server start and is not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                CacheFileName = Config.BindConfigInOrder<string>(section, "CacheFileName", "local-{world}-{seed}", "Template for the cache file name (server-resolved). Supports placeholders like {world} and {seed}. Read on world/server start and not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                CacheFileCryptKey = Config.BindConfigInOrder<string>(section, "CacheFileCryptKey", "", "Key used to obfuscate the cache contents. This is NOT secure encryption - do NOT use real passwords or personal secrets! Read on world/server start and not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });


                section = Section_Server_Gameplay;

                DefaultStarvingGraceFactor = Config.BindConfigInOrder<float>(section, "DefaultStarvingGraceFactor", 5f, "Global fallback StarvingGraceFactor used when a creature does not define one in YAML. Multiplies fed duration before entering Starving state.", acceptableValues: new AcceptableValueRange<float>(0, 100), synced: true);

                GlobalPregnancyDurationFactor = Config.BindConfigInOrder<float>(section, "GlobalPregnancyDurationFactor", 1f, "Global multiplier for PregnancyDuration.  Applies immediately; lowering it can instantly finish ongoing pregnancy on the next update.", synced: true);
                GlobalPregnancyDurationFactor.SettingChanged += (object sender, EventArgs args) => {
                    //if (ZNet.instance && ZNet.instance.IsServer())
                    {
                        foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                        {
                            var procreation = baseAI.GetComponent<Procreation>();
                            if (procreation != null)
                            {
                                procreation.UpdatePregnancyDuration();
                            }
                        }
                    }
                };

                GlobalFedDurationFactor = Config.BindConfigInOrder<float>(section, "GlobalFedDurationFactor", 1f, "Global multiplier for FedDuration. Applies immediately (may flip Hungry/Starving state).", synced: true);
                GlobalFedDurationFactor.SettingChanged += (object sender, EventArgs args) => {
                    //if (ZNet.instance && ZNet.instance.IsServer())
                    {
                        foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                        {
                            var tameable = baseAI.GetComponent<Tameable>();
                            if (tameable != null)
                            {
                                tameable.UpdateFedDuration();
                            }
                        }
                    }
                };

                GlobalTamingTimeFactor = Config.BindConfigInOrder<float>(section, "GlobalTamingTimeFactor", 1f, "Global multiplier for TamingTime. Applies immediately; lowering it can instantly finish ongoing taming on the next update.", synced: true);
                GlobalTamingTimeFactor.SettingChanged += (object sender, EventArgs args) => {
                    //if (ZNet.instance && ZNet.instance.IsServer())
                    {
                        foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                        {
                            var tameable = baseAI.GetComponent<Tameable>();
                            if (tameable != null)
                            {
                                tameable.UpdateTamingTime();
                            }
                        }
                    }
                };

                GlobalGrowTimeFactor = Config.BindConfigInOrder<float>(section, "GlobalGrowTimeFactor", 1f, "Global multiplier for egg hatching and offspring grow-up time. Applies immediately; lowering it can trigger instant hatching/growing on next update.", synced: true);
                GlobalGrowTimeFactor.SettingChanged += (object sender, EventArgs args) => {
                    //if (ZNet.instance && ZNet.instance.IsServer())
                    {
                        foreach (var itemDrop in ItemDropExtensions.GetInstances().ToArray())
                        {
                            var eggGrow = itemDrop.GetComponent<EggGrow>();
                            if (eggGrow != null)
                            {
                                eggGrow.UpdateGrowTime();
                            }
                        }
                        foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                        {
                            var growup = baseAI.GetComponent<Growup>();
                            if (growup != null)
                            {
                                growup.UpdateGrowTime();
                            }
                        }
                    }
                };

                RequireFoodDroppedByPlayer = Config.BindConfigInOrder<bool>(section, "RequireFoodDroppedByPlayer", true, "When disabled, animals will also be tamed and stimulated to breed using food found in the world (not dropped by players).", synced: true);
                UseBetterSearchForFood = Config.BindConfigInOrder<bool>(section, "UseBetterSearchForFood", true, "Uses a weighted food search instead of always picking the nearest item, resulting in more natural and less robotic animal behavior.", synced: true);
                TamingSlowdownPerStar = Config.BindConfigInOrder<float>(section, "TamingSlowdownPerStar", 1f, "Slows down taming progress per star by reducing how much progress is applied each update.\n" +
                    "This does not change the base taming time itself — it only affects how fast taming progresses.\nA value of 0 disables this feature.\n" +
                    "Formula: progress /= (1 + stars × value)\n" +
                    "Example (base time = 100s, value = 1.0): 0★ = 100s, 1★ = 200s, 2★ = 300s", synced: true);


                section = Section_Server_CLLC;

                CLLC_Infusion_WeightDirectParent = Config.BindConfigInOrder<float>(section, "Infusion_WeightDirectParent", 60, "Weight for inheriting infusion from the direct parent creature.", synced: true);
                CLLC_Infusion_WeightCurrentPartner = Config.BindConfigInOrder<float>(section, "Infusion_WeightCurrentPartner", 20, "Weight for inheriting infusion from the current breeding partner.", synced: true);
                CLLC_Infusion_WeightAnyNearby = Config.BindConfigInOrder<float>(section, "Infusion_WeightAnyNearby", 10, "Weight for inheriting infusion from any nearby eligible creature within search range.", synced: true);
                CLLC_Infusion_WeightRandomNew = Config.BindConfigInOrder<float>(section, "Infusion_WeightRandomNew", 5, "Weight for assigning a completely new random infusion.", synced: true);
                CLLC_Infusion_WeightNone = Config.BindConfigInOrder<float>(section, "Infusion_WeightNone", 5, "Weight for applying no infusion at all.", synced: true);

                CLLC_Infusion_SearchRange = Config.BindConfigInOrder<float>(section, "Infusion_SearchRange", 10, "Maximum search radius (in meters) for nearby creatures considered when inheriting infusion.", synced: true);

                CLLC_Effect_WeightDirectParent = Config.BindConfigInOrder<float>(section, "Effect_WeightDirectParent", 60, "Weight for inheriting effect from the direct parent creature.", synced: true);
                CLLC_Effect_WeightCurrentPartner = Config.BindConfigInOrder<float>(section, "Effect_WeightCurrentPartner", 20, "Weight for inheriting effect from the current breeding partner.", synced: true);
                CLLC_Effect_WeightAnyNearby = Config.BindConfigInOrder<float>(section, "Effect_WeightAnyNearby", 10, "Weight for inheriting effect from any nearby eligible creature within search range.", synced: true);
                CLLC_Effect_WeightRandomNew = Config.BindConfigInOrder<float>(section, "Effect_WeightRandomNew", 5, "Weight for assigning a completely new random effect.", synced: true);
                CLLC_Effect_WeightNone = Config.BindConfigInOrder<float>(section, "Effect_WeightNone", 5, "Weight for applying no effect at all.", synced: true);

                CLLC_Effect_SearchRange = Config.BindConfigInOrder<float>(section, "Effect_SearchRange", 10, "Maximum search radius (in meters) for nearby creatures considered when inheriting effects.", synced: true);

            }
        }
    }
}
