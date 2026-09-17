using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class EggGrowTrait : OTABComponent<EggGrowTrait>
    {

        public class EggGrown : Common.WeightedRandom.IWeighted
        {
            public float Weight { get; }
            public string Prefab { get; }
            public bool Tamed { get; }
            public bool ShowHatchEffect { get; }
            public EggGrown(
                string prefab,
                float weight,
                bool tamed,
                bool showHatchEffect)
            {
                Prefab = prefab;
                Weight = weight;
                Tamed = tamed;
                ShowHatchEffect = showHatchEffect;
            }
        }

        [NonSerialized] private static readonly List<List<string[]>> _requireGlobalKeys;
        [NonSerialized] private static readonly List<EggGrown[]> _grownListByIndex;

        static EggGrowTrait()
        {
            _requireGlobalKeys = new List<List<string[]>>();
            _grownListByIndex = new List<EggGrown[]>();

            Network.NetworkSessionManager.OnSessionClosed += () => {
                _requireGlobalKeys.Clear();
                _grownListByIndex.Clear();
            };
        }

        private static readonly Dictionary<Heightmap.Biome, string> biomeLangKeys = new Dictionary<Heightmap.Biome, string>() {
            { Heightmap.Biome.Meadows,      "$biome_meadows" },
            { Heightmap.Biome.Swamp,        "$biome_swamp" },
            { Heightmap.Biome.Mountain,     "$biome_mountain" },
            { Heightmap.Biome.BlackForest,  "$biome_blackforest" },
            { Heightmap.Biome.Plains,       "$biome_plains" },
            { Heightmap.Biome.AshLands,     "$biome_ashlands" },
            { Heightmap.Biome.DeepNorth,    "$biome_deepnorth" },
            { Heightmap.Biome.Ocean,        "$biome_ocean" },
            { Heightmap.Biome.Mistlands,    "$biome_mistlands" },
        };

        // set in Start
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private EggGrow m_eggGrow = null;
        [NonSerialized] private ItemDrop m_itemDrop = null;
        [NonSerialized] private ItemDropTrait m_itemDropTrait = null;
        [NonSerialized] private float m_baseGrowTime = 60;

        // set in registration
        [SerializeField] public Heightmap.Biome m_requireBiome = Heightmap.Biome.None;
        [SerializeField] public Utilities.EnvironmentUtils.LiquidTypeEx m_requireLiquid = Utilities.EnvironmentUtils.LiquidTypeEx.None;
        [SerializeField] public float m_requireLiquidDepth = 0;
        [SerializeField] private int m_requireGlobalKeysIndex = -1;
        [SerializeField] private int m_grownListIndex = -1;

        private void Awake()
        {
            Register(this);
        }

        private void Start()
        {
            m_nview = GetComponent<ZNetView>();
            m_eggGrow = GetComponent<EggGrow>();
            m_itemDrop = GetComponent<ItemDrop>();
            m_itemDropTrait = GetComponent<ItemDropTrait>();

            m_baseGrowTime = m_eggGrow.m_growTime;

            if (m_nview.IsValid())
            {
                m_nview.Register<float>("RPC_UpdateEffects", RPC_UpdateEffects);
                m_nview.Register<bool>("RPC_HatchAndDestroy", RPC_HatchAndDestroy);
            }

            UpdateGrowTime();
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public float GetBaseGrowTime()
        {
            return m_baseGrowTime;
        }

        internal void SetRequiredGlobalKeys(List<string[]> orKeysList)
        {
            m_requireGlobalKeysIndex = _requireGlobalKeys.Count;
            _requireGlobalKeys.Add(orKeysList);
        }

        public bool HasRequiredGlobalKeys(out List<string[]> orKeysList)
        {
            if (m_requireGlobalKeysIndex != -1)
            {
                orKeysList = _requireGlobalKeys[m_requireGlobalKeysIndex];
                if (orKeysList.Count > 0)
                {
                    return true;
                }
            }
            orKeysList = null;
            return false;
        }

        public void SetCustomGrownList(EggGrown[] grownList)
        {
            m_grownListIndex = _grownListByIndex.Count;
            _grownListByIndex.Add(grownList);
        }

        public bool HasCustomGrownList(out EggGrown[] grownList)
        {
            if (m_grownListIndex != -1)
            {
                grownList = _grownListByIndex[m_grownListIndex];
                return true;
            }
            grownList = null;
            return false;
        }

        public bool SolvesRequiredGlobalKeys()
        {
            if (HasRequiredGlobalKeys(out List<string[]> orKeysList))
            {
                return SolvesRequiredGlobalKeys(orKeysList);
            }
            return true;
        }

        private bool SolvesRequiredGlobalKeys(List<string[]> orKeysList)
        {
            foreach (var andKeys in orKeysList)
            {
                if (andKeys.All((key) => ZoneSystem.instance.GetGlobalKey(key)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool InValidBiome(Vector3 position)
        {
            if (m_requireBiome == Heightmap.Biome.None)
            {
                return true;
            }
            return Utilities.EnvironmentUtils.IsInBiome(position, m_requireBiome);
        }

        public bool OnValidGround(Vector3 position)
        {
            if (m_requireLiquid == EnvironmentUtils.LiquidTypeEx.None)
            {
                return true;
            }
            var liquidType = m_requireLiquid;
            var liquidDepth = m_requireLiquidDepth;
            return liquidType switch
            {
                Utilities.EnvironmentUtils.LiquidTypeEx.Water => Utilities.EnvironmentUtils.IsInWater(position, liquidDepth),
                Utilities.EnvironmentUtils.LiquidTypeEx.Tar => Utilities.EnvironmentUtils.IsInTar(position, liquidDepth),
                // todo: lava?
                _ => true,
            };
        }

        public bool CanGrow()
        {
            if (Plugin.Configs.RequireEggsDroppedByPlayer.Value == true)
            {
                if (m_itemDropTrait.IsDroppedByPlayer() == false)
                {
                    return false;
                }
            }

            if (SolvesRequiredGlobalKeys() == false)
            {
                return false;
            }

            var eggPosition = m_eggGrow.transform.position;

            if (InValidBiome(eggPosition) == false)
            {
                return false;
            }

            if (OnValidGround(eggPosition) == false)
            {
                return false;
            }

            return true;
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

        public void RPC_HatchAndDestroy(long sender, bool showHatchEffect)
        {
            if (!m_nview.IsValid())
            {
                return;
            }
            if (showHatchEffect)
            {
                m_eggGrow.m_hatchEffect?.Create(m_eggGrow.transform.position, m_eggGrow.transform.rotation);
            }
            if (m_nview.IsOwner())
            {
                m_nview.Destroy();
            }
        }

        public string GetEggGrowProgress()
        {
            if (!m_eggGrow || !m_nview.IsValid())
            {
                return "";
            }

            var zdo = m_nview.GetZDO();

            var canGrow = m_eggGrow.CanGrow();
            if (canGrow)
            {
                var growTime = m_eggGrow.m_growTime;
                if (growTime > 0) // has a grow time
                {
                    float growStart = zdo.GetFloat(ZDOVars.s_growStart);
                    if (growStart > 0) // is already growing
                    {
                        float precision = 1f / Plugin.Configs.HudProgressPrecision.Value;
                        int decimals = Mathf.Max(0, Mathf.RoundToInt(-Mathf.Log10(precision)));

                        float remainingTime = (float)((growStart + growTime) - ZNet.instance.GetTimeSeconds());
                        float pctRaw = (1f - Mathf.Clamp01(remainingTime / growTime)) * 100f;

                        float pct = Mathf.Floor(pctRaw * precision) / precision; // no "jumping forward"
                        var pctText = pct.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture);
                        return $"({pctText}%)";
                    }
                }

                // unknown/secret grow time
                return "";
            }
            else
            {
                if (!Plugin.IsServerDataLoaded())
                {
                    return "";
                }

                // logic:
                //   (otab)DroppedByPlayer > (vanilla)itemstack > (vanilla)fire > (vanilla)roof > (otab)globalkeys > (otab)biome > (otab)liquid

                if (Plugin.Configs.RequireEggsDroppedByPlayer.Value == true)
                {
                    if (m_itemDropTrait.IsDroppedByPlayer() == false)
                    {
                        return "";
                    }
                }

                if (m_itemDrop.m_itemData.m_stack > 1)
                {
                    return Localization.instance.Localize("$item_chicken_egg_stacked");
                }

                var position = transform.position;

                if (m_eggGrow.m_requireNearbyFire && !EffectArea.IsPointInsideArea(position, EffectArea.Type.Heat, 0.5f))
                {
                    return Localization.instance.Localize("$otab_egg_requires_heat");
                }

                if (m_eggGrow.m_requireUnderRoof)
                {
                    Cover.GetCoverForPoint(position, out var coverPercentage, out var underRoof, 0.1f);
                    if (!underRoof || coverPercentage < m_eggGrow.m_requireCoverPercentige)
                    {
                        return Localization.instance.Localize("$otab_egg_requires_roof");
                    }
                }

                if (HasRequiredGlobalKeys(out List<string[]> orKeysList) && !SolvesRequiredGlobalKeys(orKeysList))
                {
                    // just take first AND-list for now
                    // todo: maybe display full list?
                    var andList = orKeysList[0].Where((key) => !ZoneSystem.instance.GetGlobalKey(key));
                    var outList = String.Join(", ", andList.Select((k) => Localization.instance.Localize($"$OTAB_require_key_{k}")));
                    return Localization.instance.Localize("$otab_egg_requires_key", outList);
                }

                if (InValidBiome(position) == false)
                {
                    var biomes = Utilities.EnvironmentUtils.UnMaskBiomes(m_requireBiome);
                    var outList = String.Join(" / ", biomes.Select((b) => Localization.instance.Localize(biomeLangKeys[b])));
                    return Localization.instance.Localize("$otab_egg_requires_biome", outList);
                }

                if (OnValidGround(position) == false)
                {
                    switch (m_requireLiquid)
                    {
                        case Utilities.EnvironmentUtils.LiquidTypeEx.Water:
                            return Localization.instance.Localize("$otab_egg_requires_water");

                        case Utilities.EnvironmentUtils.LiquidTypeEx.Tar:
                            return Localization.instance.Localize("$otab_egg_requires_tar");

                    }
                }

                // default message
                return Localization.instance.Localize("$item_chicken_egg_cold");
            }
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

            if (!m_eggGrow.CanGrow())
            {
                s_growStart = ZDOUtils.SetFloat(zdo, ZDOVars.s_growStart, 0f, s_growStart);
                m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", s_growStart);
                return true; // handled
            }

            var prefabName = Utils.GetPrefabName(m_eggGrow.gameObject.name);

            var timeSeconds = (float)ZNet.instance.GetTimeSeconds();

            if (s_growStart == 0f)
            {
                s_growStart = ZDOUtils.SetFloat(zdo, ZDOVars.s_growStart, timeSeconds, s_growStart);
                m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", s_growStart);
            }

            bool readyToHatch = timeSeconds > (s_growStart + m_eggGrow.m_growTime);
            if (readyToHatch)
            {


                GameObject grownPrefab = null;
                bool spawnTamed = true;
                bool showHatchEffect = true;

                if (HasCustomGrownList(out var grownList))
                {
                    var foundRandom = Common.WeightedRandom.FindRandom<EggGrown>(grownList, out EggGrown grownEntry);
                    if (foundRandom)
                    {
                        grownPrefab = ZNetScene.instance.GetPrefab(grownEntry.Prefab);
                        spawnTamed = grownEntry.Tamed;
                        showHatchEffect = grownEntry.ShowHatchEffect;
                    }
                }
                if (grownPrefab == null)
                {
                    grownPrefab = m_eggGrow.m_grownPrefab;
                    spawnTamed = m_eggGrow.m_tamed;
                }
                if (grownPrefab == null)
                {
                    // todo: log error
                    s_growStart = ZDOUtils.SetFloat(zdo, ZDOVars.s_growStart, 0f, s_growStart);
                    m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UpdateEffects", s_growStart);
                    return true; // handled
                }

                var position = m_eggGrow.transform.position;
                var rotation = m_eggGrow.transform.rotation;

                if (showHatchEffect)
                {
                    // just jiggle a lil bit
                    //rotation = Quaternion.Slerp(rotation, UnityEngine.Random.rotation, 0.04f);
                    //position += Vector3.up * 0.05f;
                    float jiggleYaw = UnityEngine.Random.Range(-10f, 10f);
                    rotation *= Quaternion.Euler(0f, jiggleYaw, 0f);
                }

                GameObject spawned = UnityEngine.Object.Instantiate(grownPrefab, position, rotation);
                Character spawnedCharacter = spawned.GetComponent<Character>();
                var level = m_itemDrop.m_itemData.m_quality;

                if ((bool)spawnedCharacter)
                {
                    spawnedCharacter.SetTamed(spawnTamed);

                    var spawnedCharacterTrait = spawned.GetComponent<CharacterTrait>();
                    if (spawnedCharacterTrait && spawnedCharacterTrait.m_maxLevel > 0)
                    {
                        if (level > spawnedCharacterTrait.m_maxLevel)
                        {
                            level = spawnedCharacterTrait.m_maxLevel;
                        }
                    }
                    else
                    {
                        // important todo: warning, cannot varify level
                        level = 1;
                    }
                    spawnedCharacter.SetLevel(level);
                }
                else
                {
                    var spawnedItemDrop = spawned.GetComponent<ItemDrop>();
                    if (spawnedItemDrop)
                    {
                        if (level > spawnedItemDrop.m_itemData.m_shared.m_maxQuality)
                        {
                            level = spawnedItemDrop.m_itemData.m_shared.m_maxQuality;
                        }
                        spawnedItemDrop.SetQuality(level);
                    }
                    else
                    {
                        // important todo: warning, cannot set level
                    }
                }

                Integrations.Mods.CllCBridge.PassTraits(zdo, spawned);

                m_nview.InvokeRPC( ZNetView.Everybody, "RPC_HatchAndDestroy", showHatchEffect);
                // object is beeing destroyed in RPC_HatchAndDestroy()
            }

            return true; // handled
        }

    }
}
