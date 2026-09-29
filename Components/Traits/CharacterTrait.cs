using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components.Traits
{
    public class CharacterTrait : OTABComponent<CharacterTrait>
    {

        private struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private static Character lastHoverTarget = null;
        private static float lastHoverUpdateTime = 0;
        private static string lastHoverConsumeText = "";

        [Flags]
        public enum HostilityMask : byte
        {
            None = 0,
            Never = 1,
            Attack = 2,
            Skip = 4,
        }

        public HostilityMask TamedVersusPlayerMask { get; private set; } = HostilityMask.None;
        public HostilityMask TamedVersusGroupMask { get; private set; } = HostilityMask.None;
        public HostilityMask TamedVersusFactionMask { get; private set; } = HostilityMask.None;
        public HostilityMask TamedVersusTamedMask { get; private set; } = HostilityMask.None;
        public HostilityMask TamedVersusWildMask { get; private set; } = HostilityMask.None;

        // set in Awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private ProcreationTrait m_procreationTrait = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private GrowupTrait m_growupTrait = null;

        // set in registration
        [SerializeField] public int m_maxLevel = 0;
        [SerializeField] public bool m_changeGroupWhenTamed = false;
        [SerializeField] public string m_changeGroupWhenTamedTo = "";
        [SerializeField] public bool m_changeFactionWhenTamed = false;
        [SerializeField] public bool m_tameSpawnedOnDeath = false;

        [SerializeField] public Character.Faction m_changeFactionWhenTamedTo = Character.Faction.Players;
        [SerializeField] public IsEnemyCondition m_tamedVersusPlayer = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedVersusGroup = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedVersusFaction = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedVersusTamed = IsEnemyCondition.Default;
        [SerializeField] public IsEnemyCondition m_tamedVersusWild = IsEnemyCondition.Default;
        
        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_character = GetComponent<Character>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_procreationTrait = GetComponent<ProcreationTrait>();
            m_baseAITrait = GetComponent<BaseAITrait>();
            m_growupTrait = GetComponent<GrowupTrait>();

            Register();
        }

        private void Start()
        {
            if (m_character.IsTamed())
            {
                SetTamedCharacteristics();
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        public Character GetCharacter()
        {
            return m_character;
        }

        public bool IsTamed()
        {
            return m_tameableTrait ? m_tameableTrait.IsTamed() : m_character.IsTamed();
        }

        public void UpdateHostilities()
        {
            bool isHungry = m_tameableTrait && m_tameableTrait.IsHungry(m_tameableTrait.GetBaseFedDuration() * 3); // todo: add conf for delay

            switch (m_tamedVersusPlayer)
            {
                case IsEnemyCondition.Default: TamedVersusPlayerMask = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedVersusPlayerMask = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedVersusPlayerMask = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedVersusPlayerMask = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedVersusPlayerMask = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedVersusGroup)
            {
                case IsEnemyCondition.Default: TamedVersusGroupMask = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedVersusGroupMask = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedVersusGroupMask = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedVersusGroupMask = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedVersusGroupMask = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedVersusFaction)
            {
                case IsEnemyCondition.Default: TamedVersusFactionMask = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedVersusFactionMask = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedVersusFactionMask = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedVersusFactionMask = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedVersusFactionMask = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedVersusTamed)
            {
                case IsEnemyCondition.Default: TamedVersusTamedMask = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedVersusTamedMask = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedVersusTamedMask = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedVersusTamedMask = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedVersusTamedMask = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
            switch (m_tamedVersusWild)
            {
                case IsEnemyCondition.Default: TamedVersusWildMask = HostilityMask.None; break;
                case IsEnemyCondition.Force: TamedVersusWildMask = HostilityMask.Attack; break;
                case IsEnemyCondition.Never: TamedVersusWildMask = HostilityMask.Never; break;
                case IsEnemyCondition.WhenFed: TamedVersusWildMask = !isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
                case IsEnemyCondition.WhenHungry: TamedVersusWildMask = isHungry ? HostilityMask.Attack : HostilityMask.Skip; break;
            }
        }

        public void On_RPC_SetTamed(bool tamed)
        {
            if (tamed)
            {
                m_baseAITrait.SetSpawnPoint();
                m_baseAITrait.StopPlayerHunt();
                SetTamedCharacteristics();

                if (m_tameableTrait)
                {
                    m_tameableTrait.CancelTamingUpdate();
                }
            }
            else
            {
                if (m_tameableTrait)
                {
                    m_tameableTrait.StartTamingUpdate();
                }
            }
        }
        
        public void SetTamedCharacteristics()
        {
            var m_baseAI = GetComponent<BaseAI>();

            m_baseAI.SetHuntPlayer(false);
            m_character.m_boss = false;
            m_character.m_bossEvent = "";
            m_character.m_defeatSetGlobalKey = "";

            if (m_character.m_boss == true)
            {
                EnemyHud.instance.RemoveCharacterHud(m_character);
            }

            if (m_changeGroupWhenTamed == true)
            {
                m_character.m_group = m_changeGroupWhenTamedTo;
            }

            if (m_changeFactionWhenTamed == true)
            {
                m_character.m_faction = m_changeFactionWhenTamedTo;
            }

            if (m_baseAITrait.m_idleSoundChanceWhenTamed >= 0)
            {
                m_baseAI.m_idleSoundChance = m_baseAITrait.m_idleSoundChanceWhenTamed;
            }

            m_baseAI.m_aggravatable = false;
            if (m_nview.IsOwner() && m_nview.IsValid())
            {
                ZDOUtils.SetInt(m_nview.GetZDO(), ZDOVars.s_aggravated, 0);
            }

        }




        private const float HoverNameUpdateInterval = 0.5f;
        private string m_cachedHoverName = "";
        private float m_lastHoverNameUpdate;

        public string On_GetHoverName()
        {
            var time = Time.time;
            if (time - m_lastHoverNameUpdate < HoverNameUpdateInterval)
            {
                return m_cachedHoverName;
            }
            m_lastHoverNameUpdate = time;
            m_cachedHoverName = GetHoverName();
            return m_cachedHoverName;
        }

        private string GetHoverName()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var showTamingProgress = false;
            var remainingTamingTime = 0f;

            if (m_tameableTrait && Plugin.Configs.HudShowTamingProgress.Value)
            {
                showTamingProgress = !IsTamed() && m_tameableTrait.IsTamingStarted(out remainingTamingTime);
            }

            var showGrowupProgress = false;
            var remainingGrowupTime = 0f;

            if (m_growupTrait && Plugin.Configs.HudShowOffspringGrowProgress.Value)
            {
                showGrowupProgress = m_growupTrait.IsGrowingStarted(out remainingGrowupTime);
            }

            if (!showTamingProgress && !showGrowupProgress)
            {
                return "";
            }

            var text = "";
            var multiplier = Plugin.Configs.HudProgressMultiplier;

            if (showTamingProgress)
            {
                var tameTime = m_tameableTrait.GetTameable().m_tamingTime;
                var percent = (float)(int)((1f - Mathf.Clamp01(remainingTamingTime / tameTime)) * 100f * multiplier) / multiplier;
                var percentText = percent.ToString(Plugin.Configs.HudProgressFormat, System.Globalization.CultureInfo.InvariantCulture);
                text = Localization.instance.Localize("$otab_hud_tameness", percentText);
            }

            if (showGrowupProgress)
            {
                var growTime = m_growupTrait.GetGrowup().m_growTime;
                var percent = (float)(int)((1f - Mathf.Clamp01(remainingGrowupTime / growTime)) * 100f * multiplier) / multiplier;
                var percentText = percent.ToString(Plugin.Configs.HudProgressFormat, System.Globalization.CultureInfo.InvariantCulture);
                var growupText = Localization.instance.Localize("$otab_hud_growth", percentText);
                if (text.Length != 0)
                {
                    text += " ";
                }
                text += growupText;
            }

            return text;
        }





        private const float HoverTextUpdateInterval = 0.5f;
        private string m_cachedHoverText = "";
        //private string m_lastVanillaHoverText = null;
        private float m_lastHoverTextUpdate;

        public string On_GetHoverText(string text)
        {
            var time = Time.time;
            //if (text == m_lastVanillaHoverText && time - m_lastHoverTextUpdate < HoverTextUpdateInterval)
            if (time - m_lastHoverTextUpdate < HoverTextUpdateInterval)
            {
                return m_cachedHoverText;
            }
            //m_lastVanillaHoverText = text;
            m_lastHoverTextUpdate = time;
            m_cachedHoverText = GetHoverText(text);
            return m_cachedHoverText;
        }

        private string GetHoverText(string text)
        {
            var isTamed = m_character.IsTamed();

            if (m_tameableTrait)
            {
                text = GetTameableHoverText(text, isTamed);
            }

            var growupText = GetGrowupHoverText();
            if (growupText.Length != 0)
            {
                text += "\n" + growupText;
            }

            var consumeText = GetConsumeHoverText();
            if (consumeText.Length != 0)
            {
                text += "\n" + consumeText;
            }

            if (m_tameableTrait)
            {
                var fedTimer = GetFedTimerHoverText();
                if (fedTimer.Length != 0)
                {
                    text += "\n" + fedTimer;
                }
            }

            if (m_procreationTrait && isTamed)
            {
                var procreationText = GetProcreationHoverText();
                if (procreationText.Length != 0)
                {
                    text += "\n" + procreationText;
                }
            }

            if (Plugin.IsAdmin() && Plugin.Configs.HoverShowAdminInfo.Value)
            {
                text = AddAdminHoverText(text, isTamed);
            }

            return text;
        }

        private string GetTameableHoverText(string text, bool isTamed)
        {
            if (!isTamed)
            {
                if (m_tameableTrait.IsTamingDisabled())
                {
                    return m_tameableTrait.GetName();
                }

                if (!m_tameableTrait.CanBeTamed())
                {
                    var requireGlobalKey = m_tameableTrait.m_requireGlobalKey;
                    if (!string.IsNullOrEmpty(requireGlobalKey) && !ZoneSystem.instance.GetGlobalKey(requireGlobalKey))
                    {
                        return m_tameableTrait.GetName() + "\n" + Localization.instance.Localize("$otab_taming_requires_key", Localization.instance.Localize($"$OTAB_require_key_{requireGlobalKey}"));
                    }
                    return text;
                }
            }

            if (m_tameableTrait.IsFeedingDisabled())
            {
                var hungry = Localization.instance.Localize("$hud_tamehungry");
                text = text.Replace(", " + hungry, "");
            }

            if (m_tameableTrait.m_petCommand.Length != 0)
            {
                var pet = Localization.instance.Localize("$hud_pet");
                var petCommand = Localization.instance.Localize(m_tameableTrait.m_petCommand);
                text = text.Replace("] " + pet, "] " + petCommand);
            }

            return text;
        }

        private string GetGrowupHoverText()
        {
            if (!m_growupTrait || m_growupTrait.CanGrow(out var reason))
            {
                return "";
            }

            if (reason == 1)
            {
                var requireGlobalKey = m_growupTrait.m_requireGlobalKey;
                return Localization.instance.Localize("$otab_growing_requires_key", Localization.instance.Localize($"$OTAB_require_key_{requireGlobalKey}"));
            }

            if (reason == 2)
            {
                return Localization.instance.Localize("$otab_growing_requires_fed");
            }

            return "CANNOT GROW - UNKNOWN REASON";
        }

        public string GetConsumeHoverText()
        {
            if (!Plugin.Configs.HoverShowConsumeItems.Value)
            {
                return "";
            }

            var L = Localization.instance;
            var tSecs = Time.time;

            if (lastHoverTarget != m_character || (tSecs - lastHoverUpdateTime) > 1f)
            {
                lastHoverTarget = m_character;
                lastHoverUpdateTime = tSecs;
                lastHoverConsumeText = "";

                var displayItems = CollectConsumableDisplayItems(L);
                if (displayItems.Count == 0)
                {
                    return ""; // todo: add special prefab: "@otab:NoItem;$otab_no_item" to display "Eats nothing" text
                }

                //return string.Format(l_consumeItems, Plugin.Configs.HoverColorNormal.Value, l_empty);
                // disabled because we want to know "what creatures can eat what food" and not "what creatures cannot eat"
                // todo: remove "$otab_hover_food_empty" from translations?
                //var l_empty = L.Localize("$otab_hover_food_empty");

                // Split into multiple lines based on approx length
                const int maxLineLen = 30; // TODO: config?
                var separator = L.Localize("$otab_hover_food_separator");
                var displayLines = BuildWrappedItemLines(displayItems, separator, maxLineLen);

                // First line gets the bullet, following lines get transparent bullet
                lastHoverConsumeText = string.Join("\n", displayLines.Select((line, i) =>
                    L.Localize("$otab_hover_food", i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000", line)
                ));
            }

            return lastHoverConsumeText;
        }

        private List<ConsumableItemDisplay> CollectConsumableDisplayItems(Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();
            if ((bool)m_baseAITrait == false)
            {
                // no ai afterall? weird
                return displayItems;
            }

            if (m_baseAITrait.m_consumeItems != null )
            {
                if (m_baseAITrait.m_consumeItems.Length == 0)
                {
                    return displayItems;
                }
                
                // hard values seems to be better
                float min = 0.33f; // consumeItems.Last().fedDurationFactor;
                float max = 3.00f; // consumeItems.First().fedDurationFactor;

                var fedTimerDisabled = m_tameableTrait && m_tameableTrait.IsFeedingDisabled();

                foreach (var item in m_baseAITrait.m_consumeItems)
                {
                    var displayName = L.Localize(item.itemDrop.m_itemData.m_shared.m_name);
                    var displayColor = Utilities.ColorUtils.GetColorBetween(
                        Plugin.Configs.HoverColorBad.Value,
                        Plugin.Configs.HoverColorNormal.Value,
                        Plugin.Configs.HoverColorGood.Value,
                        Plugin.Configs.HoverColorPassive.Value,
                        fedTimerDisabled ? 0 : item.fedDurationFactor,
                        min,
                        max
                    );
                    displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
                }

                return displayItems;
            }

            List<ItemDrop> consumeItems = null;
            if (m_monsterAI)
            {
                consumeItems = m_monsterAI.m_consumeItems;
            }
            else if (m_animalAITrait)
            {
                // otab feature
                // no need to check if otab data has been loaded
                // if no data loaded m_consumeItems is just empty
                consumeItems = m_animalAITrait.m_consumeItems;
                // wait... if its an animalAI it should be handled by HasCustomConsumeItems()
                // well... use this as fallback
            }

            if (consumeItems != null && consumeItems.Count > 0)
            {
                string color = Plugin.Configs.HoverColorNormal.Value;
                foreach (var itemDrop in consumeItems)
                {
                    var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                    displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
                }
            }

            return displayItems;
        }

        private List<string> BuildWrappedItemLines(List<ConsumableItemDisplay> displayItems, string separator, int maxLineLen)
        {
            var displayLines = new List<string>();
            var displayLineItems = new List<string>();
            int lineLength = 0;

            foreach (var displayItem in displayItems)
            {
                string displayName = displayItem.Name;
                string displayColor = displayItem.Color;

                if (string.IsNullOrEmpty(displayName))
                {
                    continue;
                }

                // only count name length (no separator)
                lineLength += displayName.Length;

                displayLineItems.Add($"<color={displayColor}>{displayName}</color>");

                // flush AFTER adding
                if (lineLength >= maxLineLen)
                {
                    displayLines.Add(string.Join(separator, displayLineItems));
                    displayLineItems.Clear();
                    lineLength = 0;
                }
            }

            // flush remaining
            if (displayLineItems.Count > 0)
            {
                displayLines.Add(string.Join(separator, displayLineItems));
                displayLineItems.Clear();
                lineLength = 0;
            }

            return displayLines;
        }







        private string GetFedTimerHoverText()
        {
            if (!m_tameableTrait || !Plugin.Configs.HoverShowFedTimer.Value || m_tameableTrait.IsFeedingDisabled() || !m_nview.IsValid())
            {
                return "";
            }

            var secondsFedLeft = m_tameableTrait.GetFedTimeLeft();
            if (secondsFedLeft <= 0)
            {
                return "";
            }

            return FormatRelativeTime(
                secondsFedLeft,
                labelPositive: "$otab_hover_fed",
                labelPositiveAlt: "$otab_hover_fed_alt",
                labelNegative: "$otab_hover_hungry",
                labelNegativeAlt: "$otab_hover_hungry_alt",
                colorPositive: Plugin.Configs.HoverColorGood.Value,
                colorNegative: Plugin.Configs.HoverColorBad.Value
            );
        }

        private string GetProcreationHoverText()
        {
            if (!m_procreationTrait)
            {
                return "";
            }

            var procreation = m_procreationTrait.GetProcreation();
            if (procreation.IsPregnant())
            {
                var zdo = m_nview.GetZDO();
                var pregnantTime = new DateTime(zdo.GetLong(ZDOVars.s_pregnant, 0L));
                var duration = m_procreationTrait.GetRealPregnancyDuration();
                var secondsLeft = duration - (ZNet.instance.GetTime() - pregnantTime).TotalSeconds;
                return FormatRelativeTime(
                    secondsLeft,
                    labelPositive: "$otab_hover_pregnancy_due",
                    labelPositiveAlt: "$otab_hover_pregnancy_due_alt",
                    labelNegative: "$otab_hover_pregnancy_overdue",
                    labelNegativeAlt: "$otab_hover_pregnancy_overdue_alt",
                    colorPositive: Plugin.Configs.HoverColorGood.Value,
                    colorNegative: Plugin.Configs.HoverColorBad.Value
                );
            }

            if (!Plugin.Configs.HoverShowLovePoints.Value ||
                procreation.m_requiredLovePoints == 0)
            {
                return "";
            }

            var lovePoints = procreation.GetLovePoints();
            var color = lovePoints > 0 ? Plugin.Configs.HoverColorGood.Value : Plugin.Configs.HoverColorBad.Value;
            return Localization.instance.Localize(
                "$otab_hover_love_points",
                color,
                lovePoints.ToString(),
                procreation.m_requiredLovePoints.ToString()
            );
        }






        private string AddAdminHoverText(string text, bool isTamed)
        {
            var adminText = Localization.instance.Localize("$otab_hover_admin_info", "Prefab: " + gameObject.name);

            if (m_baseAITrait)
            {
                var info = m_baseAITrait.GetAdminHoverInfoText();
                if (info.Length != 0)
                {
                    adminText += "<size=33%>\n\n</size>" + info.Trim();
                }
            }

            if (m_tameableTrait)
            {
                var info = m_tameableTrait.GetAdminHoverInfoText();
                if (info.Length != 0)
                {
                    adminText += "<size=33%>\n\n</size>" + info.Trim();
                }
            }

            if (m_procreationTrait && isTamed)
            {
                var info = m_procreationTrait.GetAdminHoverInfoText();
                if (info.Length != 0)
                {
                    adminText += "<size=33%>\n\n</size>" + info.Trim();
                }
            }

            return text + "\n" + adminText;
        }





        private static string FormatRelativeTime(double secondsLeft, string labelPositive, string labelPositiveAlt, string labelNegative, string labelNegativeAlt, string colorPositive, string colorNegative)
        {
            var isNegative = secondsLeft < 0;
            var totalSeconds = Math.Abs(secondsLeft);

            var color = isNegative ? colorNegative : colorPositive;
            var label = isNegative ? labelNegative : labelPositive;
            var labelAlt = isNegative ? labelNegativeAlt : labelPositiveAlt;

            TimeSpan time;

            if (Plugin.Configs.HoverUseIngameTime.Value)
            {
                var secondsPerDay = EnvMan.instance.m_dayLengthSec;

                var days = (int)(totalSeconds / secondsPerDay);
                totalSeconds -= days * secondsPerDay;

                var hours = (int)(totalSeconds / 3600.0);
                totalSeconds -= hours * 3600.0;

                var minutes = (int)(totalSeconds / 60.0);
                var seconds = (int)(totalSeconds % 60.0);

                time = new TimeSpan(days, hours, minutes, seconds);
            }
            else
            {
                time = TimeSpan.FromSeconds(totalSeconds);
            }

            var localization = Localization.instance;
            var timeString = "";

            if (time.Days > 0)
            {
                timeString += localization.Localize(
                    "$otab_hover_time_days",
                    color,
                    time.Days.ToString());
            }

            var timeFormat = localization.Localize("$otab_hover_time_format");

            if (time.Hours > 0 || timeString.Length != 0)
            {
                timeString += localization.Localize("$otab_hover_time_hours", color, string.Format(timeFormat, time.Hours));
            }

            if (time.Minutes > 0 || timeString.Length != 0)
            {
                timeString += localization.Localize("$otab_hover_time_minutes", color, string.Format(timeFormat, time.Minutes));
            }

            if (Plugin.Configs.HoverShowSeconds.Value)
            {
                timeString += localization.Localize("$otab_hover_time_seconds", color, string.Format(timeFormat, time.Seconds));
            }

            return timeString.Length != 0
                ? localization.Localize(label, color, timeString.Trim())
                : localization.Localize(labelAlt, color);
        }




    }
}
