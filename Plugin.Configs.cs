using BepInEx.Configuration;
using Jotunn.Extensions;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Runtime;
using System;
using static OfTamingAndBreeding.Plugin;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        public class Configs
        {

            // -----------------------------------------------------------------
            private const string Section_UI_Hovertext = "UI - Hovertext";
            // -----------------------------------------------------------------

            public static ConfigEntry<bool> HoverShowAdminInfo { get; private set; }

            public static ConfigEntry<bool> HoverShowConsumeItems { get; private set; }
            public static ConfigEntry<bool> HoverShowLovePoints { get; private set; }
            public static ConfigEntry<bool> HoverShowPregnancyTimer { get; private set; }
            public static ConfigEntry<bool> HoverShowFedTimer { get; private set; }

            public static ConfigEntry<bool> HoverShowSeconds { get; private set; }
            public static ConfigEntry<bool> HoverUseIngameTime { get; private set; }

            public static ConfigEntry<string> HoverColorGood { get; private set; }
            public static ConfigEntry<string> HoverColorNormal { get; private set; }
            public static ConfigEntry<string> HoverColorBad { get; private set; }
            public static ConfigEntry<string> HoverColorPassive { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_UI_Hud = "UI - Hud";
            // -----------------------------------------------------------------

            public static ConfigEntry<bool> HudShowEggGrowProgress { get; private set; }
            public static ConfigEntry<bool> HudShowTamingProgress { get; private set; }
            public static ConfigEntry<bool> HudShowOffspringGrowProgress { get; private set; }

            public static ConfigEntry<float> HudProgressPrecision { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Data = "Server - Data";
            // -----------------------------------------------------------------

            public static ConfigEntry<string> DefaultWorldDirectory { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Cache = "Server - Cache";
            // -----------------------------------------------------------------

            public static ConfigEntry<bool> WriteClientCacheFile { get; private set; }
            public static ConfigEntry<bool> WriteServerCacheFiles { get; private set; }
            public static ConfigEntry<string> CacheFileName { get; private set; }
            public static ConfigEntry<bool> ExportIconsToCache { get; private set; }
            public static ConfigEntry<bool> DumpPrefabsToCache { get; private set; }
            

            // -----------------------------------------------------------------
            private const string Section_Server_Gameplay_GlobalTimeFactors = "Server - Gameplay - Global Time Factors";
            // -----------------------------------------------------------------

            public static ConfigEntry<float> TamingTimeFactor { get; private set; }
            public static ConfigEntry<float> TamingSlowdownPerStar { get; private set; }
            public static ConfigEntry<float> PregnancyDurationFactor { get; private set; }
            public static ConfigEntry<float> EggGrowTimeFactor { get; private set; }
            public static ConfigEntry<float> GrowupTimeFactor { get; private set; }
            public static ConfigEntry<float> FedDurationFactor { get; private set; }
            public static ConfigEntry<float> HostilityHungerFactor { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Gameplay_Restrictions = "Server - Gameplay - Restrictions";
            // -----------------------------------------------------------------

            public static ConfigEntry<bool> RequireEggsDroppedByPlayer { get; private set; }
            public static ConfigEntry<bool> RequireFoodDroppedByPlayer { get; private set; }
            public static ConfigEntry<bool> PreventMatingWhileFollowing { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Gameplay_Following = "Server - Gameplay - Following";
            // -----------------------------------------------------------------

            public static ConfigEntry<float> FollowColliderRadiusFactor { get; private set; }
            public static ConfigEntry<float> FollowMinDistanceMin { get; private set; }
            public static ConfigEntry<float> FollowMinDistanceMax { get; private set; }
            public static ConfigEntry<float> FollowRunDistanceFactor { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Gameplay_TamedCharacteristics = "Server - Gameplay - Tamed Characteristics";
            // -----------------------------------------------------------------

            public static ConfigEntry<float> TamedIdleSoundChanceFactor { get; private set; }
            public static ConfigEntry<bool> TamedSummonsUseSeparateLimit { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Gameplay_Troughs = "Server - Gameplay - Troughs";
            // -----------------------------------------------------------------

            public static ConfigEntry<string> CommonFeedingContainers { get; private set; }
            public static ConfigEntry<float> ContainerSearchRangeFactor { get; private set; }


            // -----------------------------------------------------------------
            private const string Section_Server_Misc = "Server - Miscellaneous";
            // -----------------------------------------------------------------

            // ...


            // -----------------------------------------------------------------
            private const string Section_Server_CLLC = "Server - Creature Level & Loot Control";
            // -----------------------------------------------------------------

            public static ConfigEntry<float> CLLC_Infusion_WeightParent { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightRandom { get; private set; }
            public static ConfigEntry<float> CLLC_Infusion_WeightNone { get; private set; }

            public static ConfigEntry<float> CLLC_Effect_WeightParent { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightRandom { get; private set; }
            public static ConfigEntry<float> CLLC_Effect_WeightNone { get; private set; }


            // -----------------------------------------------------------------

            public static void Initialize(ConfigFile Config)
            {
                string section = "";


                // -----------------------------------------------------------------
                section = Section_UI_Hovertext;
                // -----------------------------------------------------------------

                HoverShowAdminInfo = Config.BindConfigInOrder<bool>(section, "ShowAdminInfo", true, "Allow showing admin/debug creature info in hover text for local admins or hosts only.", synced: false);

                HoverShowConsumeItems = Config.BindConfigInOrder<bool>(section, "ShowConsumeItems", true, "Allow showing consumable items in creature hover text.", synced: false);
                HoverShowLovePoints = Config.BindConfigInOrder<bool>(section, "ShowLovePoints", true, "Allow showing love points in creature hover text.", synced: false);
                HoverShowPregnancyTimer = Config.BindConfigInOrder<bool>(section, "ShowPregnancyTimer", true, "Allow showing pregnancy timer in hover text.", synced: false);
                HoverShowFedTimer = Config.BindConfigInOrder<bool>(section, "ShowFedTimer", true, "Allow showing fed timer in hover text.", synced: false);

                HoverShowSeconds = Config.BindConfigInOrder<bool>(section, "ShowSeconds", false, "Show seconds in hover timers (otherwise only days/hours/minutes).", synced: false);
                HoverUseIngameTime = Config.BindConfigInOrder<bool>(section, "UseIngameTime", true, "Format timers using Valheim in-game time (day length) instead of real time.", synced: false);

                HoverColorGood = Config.BindConfigInOrder<string>(section, "ColorGood", "#44FF00", "Color used for positive/healthy hover states (hex color/color name).", synced: false);
                HoverColorNormal = Config.BindConfigInOrder<string>(section, "ColorNormal", "#EEEEEE", "Color used for neutral hover states (hex color/color name).", synced: false);
                HoverColorBad = Config.BindConfigInOrder<string>(section, "ColorBad", "#FF4400", "Color used for negative/critical hover states (hex color/color name).", synced: false);
                HoverColorPassive = Config.BindConfigInOrder<string>(section, "ColorPassive", "#AAAAAA", "Color used for passive hover states that have no active effect. (hex color/color name).", synced: false);


                // -----------------------------------------------------------------
                section = Section_UI_Hud;
                // -----------------------------------------------------------------

                HudShowEggGrowProgress = Config.BindConfigInOrder<bool>(section, "ShowEggGrowProgress", true, "Shows egg hatching progress as a percentage next to the egg's name in the HUD (visible from distance).", synced: false);
                HudShowTamingProgress = Config.BindConfigInOrder<bool>(section, "ShowTamingProgress", true, "Shows taming progress as a percentage next to the creature's name in the HUD (visible from distance).", synced: false);
                HudShowOffspringGrowProgress = Config.BindConfigInOrder<bool>(section, "ShowOffspringGrowProgress", true, "Shows offspring growth progress as a percentage next to the creature's name in the HUD (visible from distance).", synced: false);

                HudProgressPrecision = Config.BindConfigInOrder<float>(section, "ProgressPrecision", 1f, "Controls hover progress rounding (lower = more precise). Example: 1 = 1%, 0.1 = 0.1%.", acceptableValues: new AcceptableValueList<float>(1f, 0.5f, 0.25f, 0.2f, 0.1f, 0.05f, 0.025f, 0.02f, 0.01f, 0.005f, 0.0025f, 0.002f, 0.001f), synced: false);
                UpdateHudProgressPrecision();
                HudProgressPrecision.SettingChanged += (sender, args) =>
                {
                    UpdateHudProgressPrecision();
                };


                // -----------------------------------------------------------------
                section = Section_Server_Data;
                // -----------------------------------------------------------------

                DefaultWorldDirectory = Config.BindConfigInOrder<string>(section, "DefaultWorldDirectory", "default", "Fallback world data folder name used when no folder matches the current world name. Read on world/server start.", synced: false);


                // -----------------------------------------------------------------
                section = Section_Server_Cache;
                // -----------------------------------------------------------------

                WriteClientCacheFile = Config.BindConfigInOrder<bool>(section, "WriteClientCacheFile", true, "Allow clients to create and use a local OTAB cache file. This setting is read on world/server start and is not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                WriteServerCacheFiles = Config.BindConfigInOrder<bool>(section, "WriteServerCacheFiles", true, "Write debug output on the server (cleaned YAML and an unencrypted cache file). This setting is read on world/server start and is not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                CacheFileName = Config.BindConfigInOrder<string>(section, "CacheFileName", "local-{world}-{seed}", "Template for the cache file name (server-resolved). Supports placeholders like {world} and {seed}. Read on world/server start and not synchronized during runtime.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                ExportIconsToCache = Config.BindConfigInOrder<bool>(section, "ExportIconsToCache", false, "When enabled, item icons are written to the server cache directory for customization or debug purposes.", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                DumpPrefabsToCache = Config.BindConfigInOrder<bool>(section, "DumpPrefabsToCache", false, "", synced: false, configAttributes: new ConfigurationManagerAttributes() { IsAdvanced = true });
                //todo: DumpPrefabsToCache need descr


                // -----------------------------------------------------------------
                section = Section_Server_Gameplay_GlobalTimeFactors;
                // -----------------------------------------------------------------

                TamingTimeFactor = Config.BindConfigInOrder<float>(section, "TamingTimeFactor", 1f, "Global multiplier for TamingTime. Applies immediately; lowering it can instantly finish ongoing taming on the next update.", acceptableValues: new AcceptableValueRange<float>(0, 10), synced: true);
                TamingTimeFactor.SettingChanged += (object sender, EventArgs args) => {
                    foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                    {
                        var tameableTrait = baseAI.GetComponent<TameableTrait>();
                        tameableTrait?.UpdateTamingTime();
                    }
                };

                TamingSlowdownPerStar = Config.BindConfigInOrder<float>(section, "TamingSlowdownPerStar", 0.5f, "Slows down taming progress per star by reducing how much progress is applied each update.\n" +
                    "This does not change the base taming time itself — it only affects how fast taming progresses.\nA value of 0 disables this feature.\n" +
                    "Formula: progress /= (1 + stars × value)\n" +
                    "Example (base time = 100s, value = 0.5): 0★ = 100s, 1★ = 150s, 2★ = 200s", synced: true);

                PregnancyDurationFactor = Config.BindConfigInOrder<float>(section, "PregnancyDurationFactor", 1f, "Global multiplier for PregnancyDuration.  Applies immediately; lowering it can instantly finish ongoing pregnancy on the next update.", acceptableValues: new AcceptableValueRange<float>(0, 10), synced: true);
                PregnancyDurationFactor.SettingChanged += (object sender, EventArgs args) => {
                    foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                    {
                        var procreationTrait = baseAI.GetComponent<ProcreationTrait>();
                        procreationTrait?.UpdatePregnancyDuration();
                    }
                };

                EggGrowTimeFactor = Config.BindConfigInOrder<float>(section, "EggGrowTimeFactor", 1f, "Global multiplier for egg hatching time. Applies immediately; lowering it can trigger instant hatching on next update.", acceptableValues: new AcceptableValueRange<float>(0, 10), synced: true);
                EggGrowTimeFactor.SettingChanged += (object sender, EventArgs args) => {
                    foreach (var itemDrop in ItemDropExtensions.GetInstances().ToArray())
                    {
                        var eggGrowTrait = itemDrop.GetComponent<EggGrowTrait>();
                        eggGrowTrait?.UpdateGrowTime();
                    }
                };

                GrowupTimeFactor = Config.BindConfigInOrder<float>(section, "GrowupTimeFactor", 1f, "Global multiplier for offspring grow-up time. Applies immediately; lowering it can trigger instant growing on next update.", acceptableValues: new AcceptableValueRange<float>(0, 10), synced: true);
                GrowupTimeFactor.SettingChanged += (object sender, EventArgs args) => {
                    foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                    {
                        var growupTrait = baseAI.GetComponent<GrowupTrait>();
                        growupTrait?.UpdateGrowTime();
                    }
                };

                FedDurationFactor = Config.BindConfigInOrder<float>(section, "FedDurationFactor", 1f, "Global multiplier for FedDuration. Applies immediately (may flip Hungry state).", acceptableValues: new AcceptableValueRange<float>(0, 10), synced: true);
                FedDurationFactor.SettingChanged += (object sender, EventArgs args) => {
                    foreach (var baseAI in BaseAIExtensions.GetInstances().ToArray())
                    {
                        var tameableTrait = baseAI.GetComponent<TameableTrait>();
                        tameableTrait?.UpdateFedDuration();
                    }
                };

                HostilityHungerFactor = Config.BindConfigInOrder<float>(section, "HostilityHungerFactor", 3.0f, "Multiplier for the feeding duration used to determine the hunger state for reactive hostilities. Lower values result in a shorter delay, higher values in a longer delay. Does not affect the actual feeding duration.", acceptableValues: new AcceptableValueRange<float>(0f, 1000f), synced: true);


                // -----------------------------------------------------------------
                section = Section_Server_Gameplay_Restrictions;
                // -----------------------------------------------------------------

                RequireEggsDroppedByPlayer = Config.BindConfigInOrder<bool>(section, "RequireEggsDroppedByPlayer", true, "When enabled, only eggs dropped by players can grow or hatch. When disabled, eggs found in the world can also grow or hatch.", synced: true);
                RequireFoodDroppedByPlayer = Config.BindConfigInOrder<bool>(section, "RequireFoodDroppedByPlayer", true, "When disabled, animals will also be tamed and stimulated to breed using food found in the world (not dropped by players).", synced: true);
                PreventMatingWhileFollowing = Config.BindConfigInOrder<bool>(section, "PreventMatingWhileFollowing", true, "Prevents tamed creatures from mating while they are commanded to follow a player.", synced: true);


                // -----------------------------------------------------------------
                section = Section_Server_Gameplay_Following;
                // -----------------------------------------------------------------

                FollowColliderRadiusFactor = Config.BindConfigInOrder<float>(section, "ColliderRadiusFactor", 5f, "Multiplier applied to a creature's collider radius to calculate its minimum follow distance.", acceptableValues: new AcceptableValueRange<float>(0f, 20f), synced: true);
                FollowMinDistanceMin = Config.BindConfigInOrder<float>(section, "MinDistanceMin", 3f, "Lower limit for the minimum distance a creature maintains from its follow target.", acceptableValues: new AcceptableValueRange<float>(0f, 100f), synced: true);
                FollowMinDistanceMax = Config.BindConfigInOrder<float>(section, "MinDistanceMax", 10f, "Upper limit for the minimum distance a creature maintains from its follow target.", acceptableValues: new AcceptableValueRange<float>(0f, 100f), synced: true);
                FollowRunDistanceFactor = Config.BindConfigInOrder<float>(section, "RunDistanceFactor", 3f, "Multiplier applied to the minimum follow distance to determine when a creature starts running instead of walking.", acceptableValues: new AcceptableValueRange<float>(1f, 20f), synced: true);


                // -----------------------------------------------------------------
                section = Section_Server_Gameplay_TamedCharacteristics;
                // -----------------------------------------------------------------

                TamedIdleSoundChanceFactor = Config.BindConfigInOrder<float>(section, "IdleSoundChanceFactor", 0.1f, "Multiplier for the idle sound chance of tamed creatures. Range: 0 (disabled) to 1 (vanilla behavior).", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);
                TamedSummonsUseSeparateLimit = Config.BindConfigInOrder<bool>(section, "SeparateTamedSummonLimit", true, "Separates the summon limit for tamed and untamed creatures. Each group shares its own limit across all summoners.", synced: true);


                // -----------------------------------------------------------------
                section = Section_Server_Gameplay_Troughs;
                // -----------------------------------------------------------------


                CommonFeedingContainers = Config.BindConfigInOrder<string>(section, "CommonFeedingContainers", "", "Comma-separated prefab names of regular containers that should also be usable as feeding containers.", synced: true);
                CommonFeedingContainers.SettingChanged += (object sender, EventArgs args) => {
                    ContainerFeeding.RegisterConfigContainerPrefabs(CommonFeedingContainers.Value);
                };
                ContainerFeeding.RegisterConfigContainerPrefabs(CommonFeedingContainers.Value);
                ContainerSearchRangeFactor = Config.BindConfigInOrder<float>(section, "ContainerSearchRangeFactor", 2f, "Multiplier applied to the consume search range of tamed creatures when searching for food in registered feeding containers.", acceptableValues: new AcceptableValueRange<float>(1f, 10f), synced: true);


                // -----------------------------------------------------------------
                section = Section_Server_Misc;
                // -----------------------------------------------------------------

                // ...


                // -----------------------------------------------------------------
                section = Section_Server_CLLC;
                // -----------------------------------------------------------------

                CLLC_Infusion_WeightParent = Config.BindConfigInOrder<float>(section, "Infusion_WeightParent", 66, "Weight for inheriting infusion from the parent creature.", synced: true);
                CLLC_Infusion_WeightRandom = Config.BindConfigInOrder<float>(section, "Infusion_WeightRandom", 11, "Weight for assigning a completely new random infusion.", synced: true);
                CLLC_Infusion_WeightNone = Config.BindConfigInOrder<float>(section, "Infusion_WeightNone", 22, "Weight for applying no infusion at all.", synced: true);

                CLLC_Effect_WeightParent = Config.BindConfigInOrder<float>(section, "Effect_WeightParent", 66, "Weight for inheriting effect from the parent creature.", synced: true);
                CLLC_Effect_WeightRandom = Config.BindConfigInOrder<float>(section, "Effect_WeightRandom", 11, "Weight for assigning a completely new random effect.", synced: true);
                CLLC_Effect_WeightNone = Config.BindConfigInOrder<float>(section, "Effect_WeightNone", 22, "Weight for applying no effect at all.", synced: true);

            }

            public static float HudProgressMultiplier { get; private set; }
            public static string HudProgressFormat { get; private set; }

            private static void UpdateHudProgressPrecision()
            {
                var precision = HudProgressPrecision.Value;
                HudProgressMultiplier = 1f / precision;
                var decimals = Math.Max(0, (int)Math.Round(-Math.Log10(precision)));
                HudProgressFormat = $"F{decimals}";
            }

        }
    }
}
