using OfTamingAndBreeding.Internals.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{

    internal class TameableAPI : API.Tameable
    {

        private static readonly ConditionalWeakTable<Tameable, TameableAPI> instances
            = new ConditionalWeakTable<Tameable, TameableAPI>();
        public static TameableAPI GetOrCreate(Tameable __instance)
            => instances.GetValue(__instance, (Tameable inst) => new TameableAPI(inst));
        public static bool TryGetAPI(Tameable __instance, out TameableAPI api)
            => instances.TryGetValue(__instance, out api);

        //public Data.Models.Creature creatureData;
        public float lastCommandTime = 0;

        public TameableAPI(Tameable __instance) : base(__instance)
        {
            //var prefabName = Utils.GetPrefabName(__instance.name);
            //this.creatureData = Data.Models.Creature.Get(prefabName);
        }

        #region tameable animals

        public AnimalAIAPI animalAIAPI;

        public void TamingAnimalUpdate()
        {
            if (m_nview.IsValid() && m_nview.IsOwner() && !IsTamed() && !IsHungry() && !animalAIAPI.IsAlerted())
            {
                DecreaseRemainingTime(3f);
                if (GetRemainingTime() <= 0f)
                {
                    Tame();
                }
                else
                {
                    m_sootheEffect.Create(transform.position, transform.rotation);
                }
            }
        }

        public void TameAnimal()
        {
            Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
            if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !IsTamed())
            {
                animalAIAPI.MakeTame();
                m_tamedEffect.Create(transform.position, transform.rotation);
                Player closestPlayer = Player.GetClosestPlayer(transform.position, 30f);
                if ((bool)closestPlayer)
                {
                    closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                }
            }
        }

        #endregion











        struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        private string lastItemsList = "";
        private DateTime lastItemsListUpdate = DateTime.MinValue;




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

            // -------------------------------------------------
            // Consume items (cached, update max once per second)
            // -------------------------------------------------
            if ((zTime - lastItemsListUpdate).TotalSeconds >= 1.0)
            {
                lastItemsList = BuildConsumeItemsBlock(L);
                lastItemsListUpdate = zTime; // IMPORTANT: update only when rebuilt
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
                secLeft = -m_monsterAI.GetTimeSinceSpawned().TotalSeconds;
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
            if (animalAI && Internals.AnimalAIAPI.TryGetAPI(animalAI, out var api))
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
                AddSingleYamlItem(displayItems, consumeItems[0], L, Plugin.Configs.HoverColorNormal.Value);
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

        private void AddSingleYamlItem(List<ConsumableItemDisplay> displayItems, Data.Models.Creature.MonsterAIConsumItemData item, Localization L, string color)
        {
            var itemDrop = Patches.Contexts.DataContext.GetItemDropByPrefab(item.Prefab);
            if (itemDrop == null) return;

            var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
            displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
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








        /*
        public IReadOnlyList<string> GetFeedingHoverText()
        {
            ZDO zdo = m_nview.GetZDO();
            if (zdo == null)
            {
                return new string[] { };
            }

            var returnLines = new List<string>();

            var L = Localization.instance;

            var zTime = ZNet.instance.GetTime();

            var lastFedTimeLong = zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L);
            DateTime dateTime = new DateTime(lastFedTimeLong);
            var duration = m_fedDuration;
            double secLeft = duration - (zTime - dateTime).TotalSeconds;

            if (Plugin.Configs.HoverShowFedTimer.Value)
            {

                if (secLeft > 0 || Plugin.Configs.HoverShowFedTimerStarving.Value)
                {
                    if (lastFedTimeLong == 0)
                    {
                        secLeft = -m_monsterAI.GetTimeSinceSpawned().TotalSeconds;
                    }
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
            }






            if ((zTime - lastItemsListUpdate).TotalSeconds >= 1.0)
            {
                var displayItems = new List<ConsumableItemDisplay>();

                var data = Data.Models.Creature.Get(Utils.GetPrefabName(gameObject.name));
                if (data != null && data.MonsterAI != null && data.MonsterAI.ConsumeItems != null)
                {
                    if (data.MonsterAI.ConsumeItems.Length == 1)
                    {
                        var item = data.MonsterAI.ConsumeItems[0];
                        var itemDrop = Patches.Contexts.DataContext.GetItemDropByPrefab(item.Prefab);
                        var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                        var displayColor = Plugin.Configs.HoverColorNormal.Value;
                        displayItems.Add(new ConsumableItemDisplay
                        {
                            Name = displayName,
                            Color = displayColor,
                        });
                    }
                    else if (data.MonsterAI.ConsumeItems.Length > 1)
                    {
                        var min = data.MonsterAI.ConsumeItems.Last().FedDurationMultiply;
                        var max = data.MonsterAI.ConsumeItems.First().FedDurationMultiply;

                        foreach (var item in data.MonsterAI.ConsumeItems)
                        {
                            var displayColor = Helpers.ColorHelper.GetColorBetween(
                                Plugin.Configs.HoverColorBad.Value,
                                Plugin.Configs.HoverColorNormal.Value,
                                Plugin.Configs.HoverColorGood.Value,
                                item.FedDurationMultiply,
                                min,
                                max
                            );
                            var itemDrop = Patches.Contexts.DataContext.GetItemDropByPrefab(item.Prefab);
                            var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                            displayItems.Add(new ConsumableItemDisplay
                            {
                                Name = displayName,
                                Color = displayColor,
                            });
                        }
                    }
                }
                else
                {
                    var monsterAI = GetComponent<MonsterAI>();
                    if ((bool)monsterAI)
                    {
                        if (monsterAI.m_consumeItems != null)
                        {
                            foreach (var itemDrop in monsterAI.m_consumeItems)
                            {
                                var displayColor = Plugin.Configs.HoverColorNormal.Value;
                                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                                displayItems.Add(new ConsumableItemDisplay
                                {
                                    Name = displayName,
                                    Color = displayColor,
                                });
                            }
                        }
                    }
                    else
                    {
                        var animalAI = GetComponent<AnimalAI>();
                        if (animalAI)
                        {
                            if (Internals.AnimalAIAPI.TryGetAPI(animalAI, out Internals.AnimalAIAPI api))
                            {
                                if (api.m_consumeItems != null)
                                {
                                    foreach (var itemDrop in api.m_consumeItems)
                                    {
                                        var displayColor = Plugin.Configs.HoverColorNormal.Value;
                                        var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                                        displayItems.Add(new ConsumableItemDisplay
                                        {
                                            Name = displayName,
                                            Color = displayColor,
                                        });
                                    }
                                }
                            }
                        }
                    }
                }



                var l_consumeItems = L.Localize("$otab_hover_consumeItems");
                var l_consumeItems_seperator = L.Localize("$otab_hover_consumeItems_seperator");
                var l_consumeItems_empty = L.Localize("$otab_hover_consumeItems_empty");

                

                var displayLines = new List<string>();
                var displayLineItems = new List<string>();
                var lineLength = 0;
                foreach(var displayItem in displayItems)
                {
                    var displayColor = displayItem.Color;
                    var displayName = displayItem.Name;
                    lineLength += displayName.Length;

                    displayLineItems.Add($"<color={displayColor}>{displayName}</color>");
                    if (lineLength >= 25) // todo: maybe add config for this?
                    {
                        displayLines.Add(String.Join(l_consumeItems_seperator, displayLineItems));
                        displayLineItems.Clear();
                        lineLength = 0;
                    }
                }
                if (displayLineItems.Count > 0)
                {
                    displayLines.Add(String.Join(l_consumeItems_seperator, displayLineItems));
                    displayLineItems.Clear();
                    lineLength = 0;
                }
                if (displayLines.Count > 0)
                {
                    lastItemsList = string.Join("\n", displayLines.Select((line, i) =>
                        string.Format(l_consumeItems, i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000", line)
                    ));
                }
                else
                {
                    lastItemsList = string.Format(l_consumeItems, Plugin.Configs.HoverColorNormal.Value, l_consumeItems_empty);
                }





            }
            returnLines.Add(lastItemsList);
            lastItemsListUpdate = zTime;
            
            return returnLines;
        }
        */


    }

}
