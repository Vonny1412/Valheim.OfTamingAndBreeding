using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class EggGrowTrait : OTABComponent<EggGrowTrait>
    {

        // set in Start
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private EggGrow m_eggGrow = null;
        [NonSerialized] private ItemDrop m_itemDrop = null;
        [NonSerialized] private float m_baseGrowTime = 60;

        private void Start()
        {
            m_nview = GetComponent<ZNetView>();
            m_eggGrow = GetComponent<EggGrow>();
            m_itemDrop = GetComponent<ItemDrop>();

            m_baseGrowTime = m_eggGrow.m_growTime;

            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_UpdateEffects", RPC_UpdateEffects);
                m_nview.Register("RPC_HatchAndDestroy", RPC_HatchAndDestroy);
            }

            UpdateGrowTime();
        }

        public float GetBaseGrowTime()
        {
            return m_baseGrowTime;
        }


        public void UpdateGrowTime()
        {
            if (!m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalGrowTimeFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //eggGrow.UpdateGrowTime(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            UpdateGrowTime(totalFactor);
        }

        private void UpdateGrowTime(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_eggGrow.m_growTime = GetBaseGrowTime() * totalFactor;
            }
        }

        public void RPC_UpdateEffects(long sender, float grow)
        {
            if (m_nview.IsValid())
            {
                m_eggGrow.UpdateEffects(grow);
            }
        }

        public void RPC_HatchAndDestroy(long sender)
        {
            if (m_nview.IsValid())
            {
                m_eggGrow.m_hatchEffect?.Create(m_eggGrow.transform.position, m_eggGrow.transform.rotation);
                if (m_nview.IsOwner())
                {
                    m_nview.Destroy();
                }
            }
        }

        private static readonly Dictionary<Heightmap.Biome, string> biomNames = new Dictionary<Heightmap.Biome, string>();
        private static string selectedLanguage = null;

        public string GetHoverExtraText()
        {
            if (!m_nview.IsValid() || !m_eggGrow)
            {
                return "";
            }

            var zdo = m_nview.GetZDO();

            string extraText = "";
            float growStart = zdo.GetFloat(ZDOVars.s_growStart);
            var canGrow = m_eggGrow.CanGrow();

            if (canGrow)
            {
                var growTime = m_eggGrow.m_growTime;
                if (growTime > 0) // has a grow time
                {
                    if (growStart > 0) // is already growing
                    {
                        float precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
                        int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

                        float remainingTime = (float)((growStart + growTime) - ZNet.instance.GetTimeSeconds());
                        float pctRaw = (1f - Mathf.Clamp01(remainingTime / growTime)) * 100f;

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
                if (m_itemDrop.m_itemData.m_stack > 1)
                {
                    extraText = Localization.instance.Localize("$item_chicken_egg_stacked");
                }
                else if (m_eggGrow.m_requireNearbyFire)
                {
                    extraText = Localization.instance.Localize("$otab_egg_requires_heat");
                }
                else if (m_eggGrow.m_requireUnderRoof)
                {
                    extraText = Localization.instance.Localize("$otab_egg_requires_roof");
                }
                else
                {
                    if (m_eggGrow.TryGetComponent<OTABEgg>(out var component))
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
                            extraText = L.Localize("$otab_egg_requires_biome", extraText);
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

            return extraText;
        }

        public bool GrowUpdate()
        {
            if (!m_nview.IsOwner())
            {
                return true; // return as handled
            }

            var zdo = m_nview.GetZDO();

            var s_growStart = zdo.GetFloat(ZDOVars.s_growStart, 0f);
            if (m_itemDrop.m_itemData.m_stack > 1)
            {
                m_eggGrow.UpdateEffects(s_growStart);
                return true; // handled
            }

            var prefabName = global::Utils.GetPrefabName(m_eggGrow.gameObject.name);

            var z_EggBehavior = zdo.GetInt(Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Unknown);
            if (z_EggBehavior == Plugin.ZDOVars.EggBehavior.Unknown)
            {
                // determine behavior

                var customEggComponent = m_eggGrow.GetComponent<OTABEgg>();
                if (customEggComponent != null && customEggComponent.HasCustomGrownList(out var grownList))
                {
                    var foundRandom = Data.Models.SubData.RandomData.FindRandom<EggData.EggGrowGrownData>(grownList, out EggData.EggGrowGrownData grownEntry);
                    if (!foundRandom) // should not happen but whatever
                    {
                        z_EggBehavior = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla, z_EggBehavior);
                        return false;
                    }

                    var z_eggGrownPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_eggGrownPrefab, grownEntry.Prefab);
                    var z_eggGrownTamed = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_eggGrownTamed, grownEntry.Tamed ? 1 : 0);
                    var z_eggShowHatchEffect = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_eggShowHatchEffect, grownEntry.ShowHatchEffect ? 1 : 0);

                    z_EggBehavior = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.OTAB, z_EggBehavior);
                }
                else
                {
                    z_EggBehavior = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_EggBehavior, Plugin.ZDOVars.EggBehavior.Vanilla);
                }

            }

            var timeSeconds = (float)ZNet.instance.GetTimeSeconds();
            if (m_eggGrow.CanGrow())
            {
                if (s_growStart == 0f)
                {
                    s_growStart = ZNetUtils.SetFloat(zdo, ZDOVars.s_growStart, timeSeconds, s_growStart);
                }
            }
            else
            {
                s_growStart = ZNetUtils.SetFloat(zdo, ZDOVars.s_growStart, 0f, s_growStart);
            }

            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", s_growStart + 0);
            //eggGrow.UpdateEffects(s_growStart);

            if (s_growStart > 0f && timeSeconds > (s_growStart + m_eggGrow.m_growTime))
            {
                var position = m_eggGrow.transform.position;
                var rotation = m_eggGrow.transform.rotation;
                var showHatchEffect = true;

                switch (z_EggBehavior)
                {

                    case Plugin.ZDOVars.EggBehavior.Vanilla:
                        {
                            if (m_eggGrow.m_grownPrefab == null)
                            {
                                Plugin.LogError($"Egg '{prefabName}#{m_nview.GetInstanceID()}' m_grownPrefab is null");
                                return true; // but return as handled
                            }
                        }
                        break;

                    case Plugin.ZDOVars.EggBehavior.OTAB:
                        {
                            var z_eggGrownPrefab = zdo.GetString(Plugin.ZDOVars.z_eggGrownPrefab, "");
                            var z_eggGrownTamed = zdo.GetInt(Plugin.ZDOVars.z_eggGrownTamed, 1);
                            var z_eggShowHatchEffect = zdo.GetInt(Plugin.ZDOVars.z_eggShowHatchEffect, 1);

                            m_eggGrow.m_grownPrefab = ZNetScene.instance.GetPrefab(z_eggGrownPrefab);
                            m_eggGrow.m_tamed = z_eggGrownTamed == 1;
                            showHatchEffect = z_eggShowHatchEffect == 1;

                            if (m_eggGrow.m_grownPrefab == null)
                            {
                                // this should not happen but just to be save!
                                Plugin.LogError($"Egg '{prefabName}#{m_nview.GetInstanceID()}' z_eggGrownPrefab '{z_eggGrownPrefab}' does not exist");
                                return true; // but return as handled
                            }
                        }
                        break;

                }

                if (showHatchEffect)
                {
                    // just jiggle a lil bit
                    rotation = Quaternion.Slerp(rotation, UnityEngine.Random.rotation, 0.08f);
                }

                GameObject spawned = UnityEngine.Object.Instantiate(m_eggGrow.m_grownPrefab, position, rotation);
                Character spawnedCharacter = spawned.GetComponent<Character>();

                if ((bool)spawnedCharacter)
                {
                    spawnedCharacter.SetTamed(m_eggGrow.m_tamed);
                    spawnedCharacter.SetLevel(m_itemDrop.m_itemData.m_quality);
                }
                else
                {
                    spawned.GetComponent<ItemDrop>()?.SetQuality(m_itemDrop.m_itemData.m_quality);
                }

                ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

                if (showHatchEffect)
                {
                    m_nview.InvokeRPC(ZNetView.Everybody, "RPC_HatchAndDestroy");
                }

                // object is beeing destroyed in RPC_HatchAndDestroy()
                //m_nview.Destroy();
            }

            return true; // handled
        }

    }
}
