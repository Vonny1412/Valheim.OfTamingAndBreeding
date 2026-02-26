using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Chat;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ItemDropExtensions
    {
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
            => ValheimAPI.ItemDrop.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ItemDrop> GetInstances()
            => ValheimAPI.ItemDrop.__IAPI_s_instances_Invoker.Get(null);

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
                ZNetUtils.SetInt(m_nview.GetZDO(), Plugin.ZDOVars.z_droppedByAnyPlayer, val);
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
            if (eggGrow && eggGrow.GetItemDrop() != null)
            {
                string extraText = "";
                float growStart = zdo.GetFloat(ZDOVars.s_growStart);
                var canGrow = eggGrow.CanGrow();

                if (canGrow)
                {
                    if (eggGrow.m_growTime > 0) // has a grow time
                    {
                        if (growStart > 0) // is already growing
                        {
                            float precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
                            int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

                            float remainingTime = (float)((growStart + eggGrow.m_growTime) - ZNet.instance.GetTimeSeconds());
                            float pctRaw = (1f - Mathf.Clamp01(remainingTime / eggGrow.m_growTime)) * 100f;

                            float pct = Mathf.Floor(pctRaw * precision) / precision; // no "jumping forward"
                            var pctText = pct.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                            extraText = $"({pctText}%)";
                        }
                    }
                    else
                    {
                        // unknown/secret grow time
                        extraText = "(?)";
                    }
                }
                else
                {
                    if (itemDrop.m_itemData.m_stack > 1)
                    {
                        extraText = Localization.instance.Localize("$item_chicken_egg_stacked");
                    }
                    else if (eggGrow.m_requireNearbyFire)
                    {
                        extraText = Localization.instance.Localize("$otab_egg_requires_heat");
                    }
                    else if (eggGrow.m_requireUnderRoof)
                    {
                        extraText = Localization.instance.Localize("$otab_egg_requires_roof");
                    }
                    else
                    {
                        if (eggGrow.TryGetComponent<OTAB_Egg>(out var component))
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

                            if (component.m_requireBiome != Heightmap.Biome.None)
                            {
                                var biomes = Utils.EnvironmentUtils.UnMaskBiomes(component.m_requireBiome);
                                extraText = String.Join(" / ", biomes.Select((b) => biomNames[b])); // list of biomes
                                extraText = String.Format(L.Localize("$otab_egg_requires_biome"), extraText);
                            }
                            else if (component.m_requireLiquid != EnvironmentUtils.LiquidTypeEx.None)
                            {
                                switch (component.m_requireLiquid)
                                {
                                    case Utils.EnvironmentUtils.LiquidTypeEx.Water:
                                        extraText = Localization.instance.Localize("$otab_egg_requires_water");
                                        break;
                                    case Utils.EnvironmentUtils.LiquidTypeEx.Tar:
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

                if (string.IsNullOrEmpty(extraText) == false)
                {
                    text = text.Substring(0, nl) + " " + extraText + text.Substring(nl);
                }
            }
        }

    }
}
