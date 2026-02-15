using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.ValheimAPI
{
    internal static class ItemDropExtensions
    {
        internal sealed class ItemDropExtraData : Lifecycle.ExtraData<ItemDrop, ItemDropExtraData>
        {
        }

        public static class DropContext
        {
            //[ThreadStatic] public static Humanoid Dropper;
            [ThreadStatic] public static int DroppedByPlayer;
            public static void Clear()
            {
                DroppedByPlayer = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this ItemDrop that)
            => LowLevel.ItemDrop.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ItemDrop> GetInstances()
            => LowLevel.ItemDrop.__IAPI_s_instances_Invoker.Get(null);

        public static bool RemoveOne_PatchPrefix(this ItemDrop itemDrop)
        {
            var m_nview = itemDrop.GetZNetView();
            if (m_nview.IsValid() && m_nview.IsOwner())
            {
                var zdo = m_nview.GetZDO();
                TameableExtensions.ConsumeContext.hasValue = true;
                TameableExtensions.ConsumeContext.lastItemDroppedByAnyPlayer = zdo.GetInt(Plugin.ZDOVars.z_droppedByAnyPlayer, 0);
                TameableExtensions.ConsumeContext.lastItemInstanceId = itemDrop.GetInstanceID();
            }
            return true;
        }

        public static void HandleItemDropped(this ItemDrop itemDrop)
        {
            var m_nview = itemDrop.GetZNetView();
            if (m_nview.IsValid() && m_nview.IsOwner())
            {
                // hint: we are inside Humanoid_DropItem_Patch
                // Humanoid.DropItem() is calling: ItemDrop itemDrop = ItemDrop.DropItem(...)
                var val = DropContext.DroppedByPlayer;
                ZNetHelper.SetInt(m_nview.GetZDO(), Plugin.ZDOVars.z_droppedByAnyPlayer, val);
            }
        }

        private static readonly Dictionary<Heightmap.Biome, string> biomNames = new Dictionary<Heightmap.Biome, string>();
        private static string selectedLanguage = null;

        public static void GetHoverText_PatchPostfix(this ItemDrop itemDrop, ref string text)
        {
            int nl = text.IndexOf('\n');
            if (nl <= 0) return;

            var nview = itemDrop.GetZNetView();
            if (!nview || !nview.IsValid()) return;
            var zdo = nview.GetZDO();
            if (zdo == null) return;

            var eggGrow = itemDrop.GetComponent<EggGrow>();
            if (eggGrow)
            {

                string extraText = "";

                if (eggGrow.m_growTime > 0) // it has a growtime
                {

                    float growStart = zdo.GetFloat(ZDOVars.s_growStart);

                    if (itemDrop.m_itemData.m_stack > 1)
                    {
                        extraText = Localization.instance.Localize("$item_chicken_egg_stacked");
                    }
                    else if (growStart <= 0f)
                    {
                        if (!eggGrow.CanGrow())
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

                                var eggPrefabName = Utils.GetPrefabName(itemDrop.gameObject.name);

                                var needsAnyBiome = Runtime.EggGrow.GetEggNeedsAnyBiome(eggPrefabName);
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
                                    var needsLiquid = Runtime.EggGrow.GetEggNeedsLiquid(eggPrefabName);
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
                        float precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
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
                    extraText = "(?)";
                }

                text = text.Substring(0, nl) + " " + extraText + text.Substring(nl);
            }
        }







    }
}
