using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{
    internal partial class TameableAPI
    {


        struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private static string lastItemsList = "";
        private static TameableAPI lastTarget = null;


        public IReadOnlyList<string> GetFeedingHoverText()
        {
            var zdo = m_nview?.GetZDO();
            if (zdo == null)
                return Array.Empty<string>();

            var L = Localization.instance;
            var zTime = ZNet.instance.GetTime();

            var returnLines = new List<string>(capacity: 2);

            // -------------------------------------------------
            // Fed timer
            // -------------------------------------------------
            AddFedTimerLineIfEnabled(returnLines, zdo, zTime, L);


            if (lastTarget != this)
            {
                lastItemsList = BuildConsumeItemsBlock(L);
                lastTarget = this;
            }


            if (!string.IsNullOrEmpty(lastItemsList))
                returnLines.Add(lastItemsList);

            return returnLines;
        }

        private void AddFedTimerLineIfEnabled(List<string> returnLines, ZDO zdo, DateTime zTime, Localization L)
        {
            if (!Plugin.Configs.HoverShowFedTimer.Value)
                return;

            long lastFedTimeLong = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);

            double secLeft;
            if (lastFedTimeLong == 0)
            {
                // never fed -> treat as "starving since spawn"
                var baseAI = GetComponent<BaseAI>(); // need to use baseAI for tameable animals
                secLeft = -baseAI.GetTimeSinceSpawned().TotalSeconds;
            }
            else
            {
                var lastFedTime = new DateTime(lastFedTimeLong);
                secLeft = m_fedDuration - (zTime - lastFedTime).TotalSeconds;
            }

            if (!(secLeft > 0 || Plugin.Configs.HoverShowFedTimerStarving.Value))
                return;

            returnLines.Add(Helpers.StringHelper.FormatRelativeTime(
                secLeft,
                labelPositive: L.Localize("$otab_hover_fed"),
                labelNegative: L.Localize("$otab_hover_starving"),
                labelAltPositive: L.Localize("$otab_hover_fed_alt"),
                labelAltNegative: L.Localize("$otab_hover_starving_alt"),
                colorPositive: Plugin.Configs.HoverColorGood.Value,
                colorNegative: Plugin.Configs.HoverColorBad.Value
            ));
        }

        private string BuildConsumeItemsBlock(Localization L)
        {
            var displayItems = CollectConsumableDisplayItems(L);

            var l_consumeItems = L.Localize("$otab_hover_food");
            var l_separator = L.Localize("$otab_hover_food_separator");
            var l_empty = L.Localize("$otab_hover_food_empty");

            if (displayItems.Count == 0)
                return string.Format(l_consumeItems, Plugin.Configs.HoverColorNormal.Value, l_empty);

            // Split into multiple lines based on approx length
            const int maxLineLen = 25; // TODO: config?
            var displayLines = BuildWrappedItemLines_OriginalBehavior(displayItems, l_separator, maxLineLen);

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

        private static List<string> BuildWrappedItemLines_OriginalBehavior(
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

                // ORIGINAL: only count name length (no separator)
                lineLength += displayName.Length;

                displayLineItems.Add($"<color={displayColor}>{displayName}</color>");

                // ORIGINAL: flush AFTER adding
                if (lineLength >= maxLineLen)
                {
                    displayLines.Add(string.Join(separator, displayLineItems));
                    displayLineItems.Clear();
                    lineLength = 0;
                }
            }

            // ORIGINAL: flush remaining
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
