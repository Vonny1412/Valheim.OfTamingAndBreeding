using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Components.Traits
{
    public class CharacterTrait : OTABComponent<CharacterTrait>
    {

        // set in Awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private AnimalAITrait m_animalAITrait = null;
        [NonSerialized] private TameableTrait m_tameableTrait = null;
        [NonSerialized] private ProcreationTrait m_procreationTrait = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;

        // set in registration
        [SerializeField] public IsEnemyCondition m_canAttackTamed = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canBeAttackedByTamed = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canAttackPlayer = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canBeAttackedByPlayer = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canAttackGroup = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canBeAttackedByGroup = IsEnemyCondition.Never;
        [SerializeField] public IsEnemyCondition m_canAttackFaction = IsEnemyCondition.Always;
        [SerializeField] public IsEnemyCondition m_canBeAttackedByFaction = IsEnemyCondition.Always;
        [SerializeField] public bool m_changeGroupWhenTamed = false;
        [SerializeField] public string m_changeGroupWhenTamedTo = "";
        [SerializeField] public bool m_changeFactionWhenTamed = false;
        [SerializeField] public Character.Faction m_changeFactionWhenTamedTo = Character.Faction.Players;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_character = GetComponent<Character>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_animalAITrait = GetComponent<AnimalAITrait>();
            m_tameableTrait = GetComponent<TameableTrait>();
            m_procreationTrait = GetComponent<ProcreationTrait>();
            m_baseAITrait = GetComponent<BaseAITrait>();

            OnTamed();
        }

        public bool IsHungry()
        {
            return m_tameableTrait && m_tameableTrait.IsHungry();
        }

        public bool IsStarving()
        {
            return m_tameableTrait && m_tameableTrait.IsStarving();
        }

        public void OnTamed()
        {
            if (m_character.IsTamed() == false)
            {
                return;
            }

            if (m_character.m_boss == true)
            {
                m_character.m_boss = false;
                m_character.m_bossEvent = "";
                EnemyHud.instance.RemoveCharacterHud(m_character);
            }

            var baseAI = m_character.GetComponent<BaseAI>();
            if (baseAI && baseAI.HuntPlayer())
            {
                // why here? because a character could be tamed without Tameable component
                baseAI.SetHuntPlayer(hunt: false);
                baseAI.SetAlerted(alerted: false);
            }

            if (m_changeGroupWhenTamed == true)
            {
                m_character.m_group = m_changeGroupWhenTamedTo;
            }
            if (m_changeFactionWhenTamed == true)
            {
                m_character.m_faction = m_changeFactionWhenTamedTo;
            }
        }

        public string EditHoverText(string text)
        {
            var isTamed = m_character.IsTamed();

            if (m_tameableTrait)
            {

                if (!isTamed && m_tameableTrait.IsTamingDisabled())
                {
                    text = m_tameableTrait.GetName();
                }
                else if (!isTamed && m_tameableTrait.CanBeTamed() == false)
                {
                    text = m_tameableTrait.GetName() + "\n" + m_tameableTrait.GetNotTameableReason();
                }
                else if (m_tameableTrait.IsFedTimerDisabled() == true)
                {
                    var hungry = Localization.instance.Localize("$hud_tamehungry");
                    if (!string.IsNullOrEmpty(hungry))
                    {
                        var idx = text.IndexOf('\n');
                        string firstLine;
                        string rest;

                        if (idx >= 0)
                        {
                            firstLine = text.Substring(0, idx);
                            rest = text.Substring(idx); // includes \n
                        }
                        else
                        {
                            firstLine = text;
                            rest = "";
                        }

                        // remove token in common placements
                        firstLine = firstLine.Replace(", " + hungry, "");
                        firstLine = firstLine.Replace(hungry + ", ", "");
                        firstLine = firstLine.Replace(hungry, "");

                        // cleanup spacing / punctuation artifacts
                        firstLine = firstLine.Replace(",  ", ", ");
                        firstLine = firstLine.Replace("  )", " )");
                        firstLine = firstLine.Replace("(  ", "( ");

                        // remove empty parentheses variants
                        firstLine = firstLine.Replace(" ( )", "");
                        firstLine = firstLine.Replace("( )", "");
                        firstLine = firstLine.Replace("()", "");

                        firstLine = firstLine.TrimEnd();

                        text = firstLine + rest;
                    }
                }
                else
                {
                    // taming enabled + eating enabled -> show fed timer
                    var fedTimer = m_tameableTrait.GetFedTimerHoverText();
                    if (fedTimer.Length != 0)
                    {
                        text += "\n" + fedTimer;
                    }
                }
            }

            var consumeText = GetConsumeHoverText();
            if (consumeText.Length != 0)
            {
                text += "\n" + consumeText;
            }
  
            if (isTamed && m_procreationTrait)
            {
                var procreationText = m_procreationTrait.GetProcreationHoverText();
                if (procreationText.Length != 0)
                {
                    text += "\n" + procreationText;
                }
            }

            return text;
        }








        private static Character lastHoverTarget = null;
        private static string lastHoverConsumeText = "";

        public string GetConsumeHoverText()
        {
            if (!Plugin.Configs.HoverShowConsumeItems.Value)
            {
                return "";
            }

            var L = Localization.instance;

            if (lastHoverTarget != m_character)
            {
                lastHoverTarget = m_character;
                lastHoverConsumeText = "";

                var displayItems = CollectConsumableDisplayItems(L);
                if (displayItems.Count == 0)
                {
                    return "";
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
                    L.Localize(
                        "$otab_hover_food",
                        i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000",
                        line
                    )
                ));
            }

            return lastHoverConsumeText;
        }

        private struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private List<ConsumableItemDisplay> CollectConsumableDisplayItems(Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();
            if ((bool)m_baseAITrait == false)
            {
                // no ai afterall? weird
                return displayItems;
            }

            // otab feature
            // no need to check if otab data has been loaded
            // if no data loaded HasCustomConsumeItems() will return false
            if (m_baseAITrait.HasCustomConsumeItems(out var customItems))
            {
                if (customItems.Length == 0)
                {
                    return displayItems;
                }

                // hard values seems to be better
                float min = 0.33f; // consumeItems.Last().fedDurationFactor;
                float max = 3.00f; // consumeItems.First().fedDurationFactor;

                foreach (var item in customItems)
                {
                    var displayName = L.Localize(item.itemDrop.m_itemData.m_shared.m_name);
                    var displayColor = OTABUtils.ColorUtils.GetColorBetween(
                        Plugin.Configs.HoverColorBad.Value,
                        Plugin.Configs.HoverColorNormal.Value,
                        Plugin.Configs.HoverColorGood.Value,
                        item.fedDurationFactor,
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

    }
}
