using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class CharacterExtensions
    {
        private static class HoverContext
        {
            // no need for threadstatic
            public static string lastText = "";
            public static Character lastTarget = null;
        }

        public static void SetCharacterStuffIfTamed(this Character character)
        {
            if (character.IsTamed())
            {

                if (character.m_boss == true)
                {
                    character.m_boss = false;
                    character.m_bossEvent = "";
                    EnemyHud.instance.RemoveCharacterHud(character);
                }

                var baseAI = character.GetComponent<BaseAI>();
                if (baseAI && baseAI.HuntPlayer())
                {
                    // why here? because a character could be tamaed without Tameable component
                    baseAI.SetHuntPlayer(hunt: false);
                    baseAI.SetAlerted(alerted: false);
                }

                if (character.TryGetComponent<OTAB_Creature>(out var custom))
                {
                    if (custom.m_changeGroupWhenTamed == true)
                    {
                        character.m_group = custom.m_changeGroupWhenTamedTo;
                    }
                    if (custom.m_changeFactionWhenTamed == true)
                    {
                        character.m_faction = custom.m_changeFactionWhenTamedTo;
                    }
                }

            }
        }

        public static void GetHoverName_PatchPostfix(this Character character, ref string result)
        {
            var precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
            int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

            var tameable = character.GetComponent<Tameable>(); // wild creature beeing tamed
            var growup = character.GetComponent<Growup>(); // offspring growing up

            if (tameable && Plugin.Configs.HudShowTamingProgress.Value)
            {
                if (character.IsTamed() == false && tameable.m_tamingTime > 0) // 0 = disable taming
                {
                    var zdo = tameable.GetZNetView().GetZDO();

                    var remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
                    if (remainingTime < tameable.m_tamingTime)
                    {
                        var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / tameable.m_tamingTime)) * 100f * precision) / precision;
                        string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                        result += " " + string.Format(Localization.instance.Localize("$otab_hud_tameness"), percentText);
                    }
                    // hotfix
                    else if (remainingTime > tameable.m_tamingTime)
                    {
                        zdo.Set(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
                    }
                }
            }

            if (growup && Plugin.Configs.HudShowOffspringGrowProgress.Value)
            {
                var m_baseAI = growup.GetBaseAI();
                if (m_baseAI)
                {
                    var remainingTime = growup.m_growTime - (float)m_baseAI.GetTimeSinceSpawned().TotalSeconds;
                    var percent = (float)(int)((1f - Mathf.Clamp01(remainingTime / growup.m_growTime)) * 100f * precision) / precision;
                    string percentText = percent.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                    result += " " + string.Format(Localization.instance.Localize("$otab_hud_growth"), percentText);
                }
            }
        }

        public static void GetHoverText_PatchPostfix(this Character character, ref string result)
        {
            var textLines = new List<string>();

            var creatureTrait = character.GetComponent<OTAB_Creature>();
            var m_tamingDisabled = creatureTrait && creatureTrait.m_tamingDisabled;
            var m_fedTimerDisabled = creatureTrait && creatureTrait.m_fedTimerDisabled;

            var prefabName = global::Utils.GetPrefabName(character.gameObject.name);
            var isTamed = character.IsTamed();
            Tameable tameable = character.GetComponent<Tameable>();
            if ((bool)tameable)
            {
                if (!isTamed && m_tamingDisabled == true)
                {
                    // todo: overwrite or not?
                    //text = tameable.GetName();
                }
                else
                {
                    if (m_fedTimerDisabled == true)
                    {
                        var hungry = Localization.instance.Localize("$hud_tamehungry");
                        if (!string.IsNullOrEmpty(hungry))
                        {
                            var idx = result.IndexOf('\n');
                            string firstLine;
                            string rest;

                            if (idx >= 0)
                            {
                                firstLine = result.Substring(0, idx);
                                rest = result.Substring(idx); // includes \n
                            }
                            else
                            {
                                firstLine = result;
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

                            result = firstLine + rest;
                        }
                    }
                    else
                    {
                        // taming enabled + eating enabled -> show fed timer
                        textLines.AddRange(tameable.GetFeedingHoverText());
                    }
                }
            }

            textLines.AddRange(character.GetConsumeHoverText());

            if (isTamed)
            {
                var procreation = character.GetComponent<Procreation>();
                if (procreation != null)
                {
                    textLines.AddRange(procreation.GetProcreationHoverText());
                }
            }

            if (textLines.Count > 0)
            {
                if (!result.EndsWith("\n"))
                {
                    result += "\n";
                }
                result += string.Join("\n", textLines.Where((string line) => line.Trim() != ""));
            }

        }

        struct ConsumableItemDisplay
        {
            public string Name;
            public string Color;
        }

        public static IReadOnlyList<string> GetConsumeHoverText(this Character character)
        {
            if (!Plugin.Configs.HoverShowConsumeItems.Value)
            {
                return Array.Empty<string>();
            }

            var L = Localization.instance;
            var returnLines = new List<string>();

            if (HoverContext.lastTarget != character)
            {
                HoverContext.lastText = character.BuildConsumeItemsBlock(L);
                HoverContext.lastTarget = character;
            }
            if (!string.IsNullOrEmpty(HoverContext.lastText))
                returnLines.Add(HoverContext.lastText);

            return returnLines;
        }

        private static string BuildConsumeItemsBlock(this Character character, Localization L)
        {
            var displayItems = character.CollectConsumableDisplayItems(L);

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
            var displayLines = character.BuildWrappedItemLines(displayItems, l_separator, maxLineLen);

            // First line gets the bullet, following lines get transparent bullet
            return string.Join("\n", displayLines.Select((line, i) =>
                string.Format(
                    l_consumeItems,
                    i == 0 ? Plugin.Configs.HoverColorNormal.Value : "#00000000",
                    line
                )
            ));
        }

        private static List<ConsumableItemDisplay> CollectConsumableDisplayItems(this Character character, Localization L)
        {
            var displayItems = new List<ConsumableItemDisplay>();

            if (Plugin.IsOTABDataLoaded())
            {
                // custom consume items with fed duration factors is an otab feature
                if (character.TryGetComponent<OTAB_Creature>(out var creature))
                {
                    if (creature.HasCustomConsumeItems(out var consumeItems))
                    {
                        character.AddDisplayItemsFromYaml(displayItems, consumeItems, L);
                        return displayItems;
                    }
                }
            }

            List<ItemDrop> m_consumeItems = null;
            var monsterAI = character.GetComponent<MonsterAI>();
            if (monsterAI)
            {
                m_consumeItems = monsterAI.m_consumeItems;
            }
            else
            {
                if (Plugin.IsOTABDataLoaded())
                {
                    var animalAI = character.GetComponent<OTAB_CustomAnimalAI>();
                    if (animalAI)
                    {
                        m_consumeItems = animalAI.m_consumeItems;
                    }
                }
            }
            if (m_consumeItems != null && m_consumeItems.Count > 0)
            {
                character.AddDisplayItemsFromItemDrops(displayItems, m_consumeItems, L, Plugin.Configs.HoverColorNormal.Value);
            }

            return displayItems;
        }

        private static void AddDisplayItemsFromYaml(this Character character, List<ConsumableItemDisplay> displayItems, StaticContext.CreatureDataContext.ConsumeItem[] consumeItems, Localization L)
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

            float min = consumeItems.Last().fedDurationFactor;
            float max = consumeItems.First().fedDurationFactor;

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

        private static void AddDisplayItemsFromItemDrops(this Character character, List<ConsumableItemDisplay> displayItems, IEnumerable<ItemDrop> itemDrops, Localization L, string color)
        {
            foreach (var itemDrop in itemDrops)
            {
                if (itemDrop == null) continue;
                var displayName = L.Localize(itemDrop.m_itemData.m_shared.m_name);
                displayItems.Add(new ConsumableItemDisplay { Name = displayName, Color = color });
            }
        }

        private static List<string> BuildWrappedItemLines(this Character character, List<ConsumableItemDisplay> displayItems, string separator, int maxLineLen)
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
