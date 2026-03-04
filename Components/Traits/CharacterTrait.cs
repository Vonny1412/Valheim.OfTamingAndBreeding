using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.Components.Traits
{
    public class CharacterTrait : OTABComponent<CharacterTrait>
    {

        // set in Awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private MonsterAI m_monsterAI = null;
        [NonSerialized] private ExtendedAnimaAI m_exAnimalAI = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_character = GetComponent<Character>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_exAnimalAI = GetComponent<ExtendedAnimaAI>();

            OnTamed();
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

            if (m_character.TryGetComponent<OTABCreature>(out var custom))
            {
                if (custom.m_changeGroupWhenTamed == true)
                {
                    m_character.m_group = custom.m_changeGroupWhenTamedTo;
                }
                if (custom.m_changeFactionWhenTamed == true)
                {
                    m_character.m_faction = custom.m_changeFactionWhenTamedTo;
                }
            }
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
                lastHoverConsumeText = BuildConsumeItemsBlock(L);
            }

            return lastHoverConsumeText;
        }

        private string BuildConsumeItemsBlock(Localization L)
        {
            var displayItems = CollectConsumableDisplayItems(L);

            if (displayItems.Count == 0)
                return "";

            //return string.Format(l_consumeItems, Plugin.Configs.HoverColorNormal.Value, l_empty);
            // disabled because we want to know "what creatures can eat what food" and not "what creatures cannot eat"
            // todo: remove "$otab_hover_food_empty" from translations?
            //var l_empty = L.Localize("$otab_hover_food_empty");

            // Split into multiple lines based on approx length
            const int maxLineLen = 30; // TODO: config?
            var separator = L.Localize("$otab_hover_food_separator");
            var displayLines = BuildWrappedItemLines(displayItems, separator, maxLineLen);

            // First line gets the bullet, following lines get transparent bullet
            return string.Join("\n", displayLines.Select((line, i) =>
                L.Localize(
                    "$otab_hover_food",
                    i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000",
                    line
                )
            ));
        }

        private struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private List<ConsumableItemDisplay> CollectConsumableDisplayItems(Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();

            if (Plugin.IsServerDataLoaded())
            {
                // otab feature
                if (m_character.TryGetComponent<OTABCreature>(out var creature))
                {
                    if (creature.HasCustomConsumeItems(out var consumeItems))
                    {
                        AddDisplayItemsFromYaml(displayItems, consumeItems, L);
                        return displayItems;
                    }
                }
            }

            List<ItemDrop> m_consumeItems = null;
            if (m_monsterAI)
            {
                m_consumeItems = m_monsterAI.m_consumeItems;
            }
            else
            {
                if (Plugin.IsServerDataLoaded())
                {
                    if (m_exAnimalAI)
                    {
                        m_consumeItems = m_exAnimalAI.m_consumeItems;
                    }
                }
            }
            if (m_consumeItems != null && m_consumeItems.Count > 0)
            {
                AddDisplayItemsFromItemDrops(displayItems, m_consumeItems, L);
            }

            return displayItems;
        }

        private void AddDisplayItemsFromYaml(List<ConsumableItemDisplay> displayItems, StaticContext.CreatureDataContext.ConsumeItem[] consumeItems, Localization L)
        {
            if (consumeItems.Length == 0)
            {
                return;
            }

            if (consumeItems.Length == 1)
            {
                var displayName = L.Localize(consumeItems[0].itemDrop.m_itemData.m_shared.m_name);
                var displayColor = Plugin.Configs.HoverColorNormal.Value;
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
                return;
            }

            float min = 0.33f; // consumeItems.Last().fedDurationFactor;
            float max = 3.00f; // consumeItems.First().fedDurationFactor;

            foreach (var item in consumeItems)
            {
                var displayName = L.Localize(item.itemDrop.m_itemData.m_shared.m_name);
                var displayColor = Utils.ColorUtils.GetColorBetween(
                    Plugin.Configs.HoverColorBad.Value,
                    Plugin.Configs.HoverColorNormal.Value,
                    Plugin.Configs.HoverColorGood.Value,
                    item.fedDurationFactor,
                    min,
                    max
                );
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
            }
        }

        private void AddDisplayItemsFromItemDrops(List<ConsumableItemDisplay> displayItems, IEnumerable<ItemDrop> itemDrops, Localization L)
        {
            string color = Plugin.Configs.HoverColorNormal.Value;
            foreach (var itemDrop in itemDrops)
            {
                if (itemDrop == null) continue;
                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
            }
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
