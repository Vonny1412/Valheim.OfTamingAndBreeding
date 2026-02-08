using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{

    internal partial class CharacterAPI
    {
        struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private static string lastItemsList = "";
        private static CharacterAPI lastTarget = null;

        public IReadOnlyList<string> GetConsumeHoverText()
        {
            var L = Localization.instance;
            var returnLines = new List<string>(capacity: 2);

            if (lastTarget != this)
            {
                lastItemsList = BuildConsumeItemsBlock(L);
                lastTarget = this;
            }
            if (!string.IsNullOrEmpty(lastItemsList))
                returnLines.Add(lastItemsList);

            return returnLines;
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

            var l_consumeItems = L.Localize("$otab_hover_food");
            var l_separator = L.Localize("$otab_hover_food_separator");

            // Split into multiple lines based on approx length
            const int maxLineLen = 30; // TODO: config?
            var displayLines = BuildWrappedItemLines(displayItems, l_separator, maxLineLen);

            // First line gets the bullet, following lines get transparent bullet
            return string.Join("\n", displayLines.Select((line, i) =>
                string.Format(
                    l_consumeItems,
                    i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000",
                    line
                )
            ));
        }

        private List<ConsumableItemDisplay> CollectConsumableDisplayItems(Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();

            // 1) Prefer OTAB YAML data (includes FedDurationMultiply coloring)
            var data = Data.Models.Creature.Get(Utils.GetPrefabName(gameObject.name));
            var consumeItems = data?.MonsterAI?.ConsumeItems;
            if (consumeItems != null && consumeItems.Length > 0)
            {
                AddDisplayItemsFromYaml(displayItems, consumeItems, L);
                return displayItems;
            }

            // 2) Fallback: vanilla MonsterAI consume items
            var monsterAI = GetComponent<MonsterAI>();
            if (monsterAI && monsterAI.m_consumeItems != null && monsterAI.m_consumeItems.Count > 0)
            {
                AddDisplayItemsFromItemDrops(displayItems, monsterAI.m_consumeItems, L, Plugin.Configs.HoverColorNormal.Value);
                return displayItems;
            }

            // 3) Fallback: AnimalAIAPI
            var animalAI = GetComponent<AnimalAI>();
            if (animalAI && Internals.AnimalAIAPI.TryGet(animalAI, out var api))
            {
                if (api.m_consumeItems != null && api.m_consumeItems.Count > 0)
                {
                    AddDisplayItemsFromItemDrops(displayItems, api.m_consumeItems, L, Plugin.Configs.HoverColorNormal.Value);
                    return displayItems;
                }
            }

            return displayItems;
        }

        private void AddDisplayItemsFromYaml(List<ConsumableItemDisplay> displayItems, Data.Models.Creature.MonsterAIConsumItemData[] consumeItems, Localization L)
        {
            if (consumeItems.Length == 1)
            {
                var itemDrop = Patches.Contexts.DataContext.GetItemDropByPrefab(consumeItems[0].Prefab);
                if (itemDrop == null) return;

                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                var displayColor = Plugin.Configs.HoverColorNormal.Value;
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
                return;
            }

            float min = consumeItems.Last().FedDurationMultiply;
            float max = consumeItems.First().FedDurationMultiply;

            foreach (var item in consumeItems)
            {
                var itemDrop = Patches.Contexts.DataContext.GetItemDropByPrefab(item.Prefab);
                if (itemDrop == null) continue;

                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                var displayColor = Helpers.ColorHelper.GetColorBetween(
                    Plugin.Configs.HoverColorBad.Value,
                    Plugin.Configs.HoverColorNormal.Value,
                    Plugin.Configs.HoverColorGood.Value,
                    item.FedDurationMultiply,
                    min,
                    max
                );
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = displayColor });
            }
        }

        private void AddDisplayItemsFromItemDrops(List<ConsumableItemDisplay> displayItems, IEnumerable<ItemDrop> itemDrops, Localization L, string color)
        {
            foreach (var itemDrop in itemDrops)
            {
                if (itemDrop == null) continue;
                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
            }
        }

        private static List<string> BuildWrappedItemLines(
            List<ConsumableItemDisplay> displayItems,
            string separator,
            int maxLineLen)
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
