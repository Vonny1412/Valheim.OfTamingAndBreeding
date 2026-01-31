using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Chat;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
    static class ItemDrop_GetHoverText_Patch
    {

        private static readonly Dictionary<Heightmap.Biome, string> biomNames = new Dictionary<Heightmap.Biome, string>();
        private static string selectedLanguage = null;


        static void Postfix(ItemDrop __instance, ref string __result)
        {
            int nl = __result.IndexOf('\n');
            if (nl <= 0) return;

            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo)) return;

            var eggGrow = __instance.GetComponent<EggGrow>();
            if (eggGrow)
            {

                string extraText = "";

                if (eggGrow.m_growTime > 0) // it has a growtime
                {

                    float growStart = zdo.GetFloat(ZDOVars.s_growStart);

                    if (__instance.m_itemData.m_stack > 1)
                    {
                        extraText = Localization.instance.Localize("$item_chicken_egg_stacked");
                    }
                    else if (growStart <= 0f)
                    {
                        var eggGrowAPI = Internals.EggGrowAPI.GetOrCreate(eggGrow);
                        if (!eggGrowAPI.CanGrow())
                        {
                            if (eggGrow.m_requireNearbyFire)
                            {
                                extraText = Localization.instance.Localize("$otab_egg_requires_heat");
                            }
                            else if (eggGrow.m_requireUnderRoof)
                            {
                                extraText = Localization.instance.Localize("$otab_egg_requires_roof");
                            }
                            else
                            {

                                var eggPrefabName = Utils.GetPrefabName(__instance.gameObject.name);

                                var needsAnyBiome = Contexts.DataContext.GetEggNeedsAnyBiome(eggPrefabName);
                                if (needsAnyBiome != Heightmap.Biome.None)
                                {
                                    var L = Localization.instance;
                                    if (selectedLanguage != L.GetSelectedLanguage())
                                    {
                                        selectedLanguage = L.GetSelectedLanguage();
                                        biomNames.Clear();
                                        biomNames[Heightmap.Biome.Meadows] = L.Localize("$biome_meadows");
                                        biomNames[Heightmap.Biome.Swamp] = L.Localize("$biome_swamp");
                                        biomNames[Heightmap.Biome.Mountain] = L.Localize("$biome_mountain");
                                        biomNames[Heightmap.Biome.BlackForest] = L.Localize("$biome_blackforest");
                                        biomNames[Heightmap.Biome.Plains] = L.Localize("$biome_plains");
                                        biomNames[Heightmap.Biome.AshLands] = L.Localize("$biome_ashlands");
                                        biomNames[Heightmap.Biome.DeepNorth] = L.Localize("$biome_deepnorth");
                                        biomNames[Heightmap.Biome.Ocean] = L.Localize("$biome_ocean");
                                        biomNames[Heightmap.Biome.Mistlands] = L.Localize("$biome_mistlands");
                                    }
                                    var biomes = Helpers.EnvironmentHelper.UnMaskBiomes(needsAnyBiome);
                                    extraText = String.Join(" / ", biomes.Select((b) => biomNames[b])); // list of biomes
                                    extraText = String.Format(L.Localize("$otab_egg_requires_biome"), extraText);
                                }
                                else
                                {
                                    var needsLiquid = Contexts.DataContext.GetEggNeedsLiquid(eggPrefabName);
                                    if (needsLiquid != null)
                                    {
                                        switch (needsLiquid.Type)
                                        {
                                            case Helpers.EnvironmentHelper.LiquidTypeEx.Water:
                                                extraText = Localization.instance.Localize("$otab_egg_requires_water");
                                                break;
                                            case Helpers.EnvironmentHelper.LiquidTypeEx.Tar:
                                                extraText = Localization.instance.Localize("$otab_egg_requires_tar");
                                                break;
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(extraText))
                            {
                                // default message
                                extraText = Localization.instance.Localize("$item_chicken_egg_cold");
                            }
                        }
                    }
                    else
                    {
                        float precision = 1f / Plugin.Configs.HoverProgressPrecision.Value;
                        int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

                        float remainingTime = (float)((growStart + eggGrow.m_growTime) - ZNet.instance.GetTimeSeconds());
                        float pctRaw = (1f - Mathf.Clamp01(remainingTime / eggGrow.m_growTime)) * 100f;

                        float pct = Mathf.Floor(pctRaw * precision) / precision; // no "jumping forward"
                        string pctText = pct.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);

                        extraText = $"({pctText}%)";
                    }

                }
                else
                {
                    // secret growing
                    extraText = $"(?)";
                }


                __result = __result.Substring(0, nl) + " " + extraText + __result.Substring(nl);
            }

        }
    }

    /** original method
    public string GetHoverText()
    {
        Load();
        string text = m_itemData.m_shared.m_name;
        if (m_itemData.m_quality > 1)
        {
            text = text + "[" + m_itemData.m_quality + "] ";
        }

        if (m_itemData.m_shared.m_itemType == ItemData.ItemType.Consumable && IsPiece())
        {
            return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (m_itemData.m_shared.m_isDrink ? "$item_drink" : "$item_eat"));
        }

        if (m_itemData.m_stack > 1)
        {
            text = text + " x" + m_itemData.m_stack;
        }

        return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
    }
    **/

}
