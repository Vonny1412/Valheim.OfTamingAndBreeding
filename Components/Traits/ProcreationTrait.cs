using OfTamingAndBreeding.Common;
using OfTamingAndBreeding.Components.Core;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


//todo: cleanup



namespace OfTamingAndBreeding.Components.Traits
{
    public class ProcreationTrait : OTABComponent<ProcreationTrait>
    {
        public class ProcreationPartner : Common.WeightedRandom.IWeighted
        {
            public float Weight { get; }
            public string Prefab { get; }
            public ProcreationPartner(string prefab, float weight)
            {
                Prefab = prefab;
                Weight = weight;
            }
        }

        public class ProcreationOffspring : Common.WeightedRandom.IWeighted
        {
            public string Prefab { get; }
            public float Weight { get; }
            public bool NeedPartner { get; }
            public string NeedPartnerPrefab { get; }
            public float LevelUpChance { get; }
            public bool SpawnTamed { get; }
            public ProcreationOffspring(
                string prefab,
                float weight,
                bool needPartner,
                string needPartnerPrefab,
                float levelUpChance,
                bool spawnTamed
            )
            {
                Prefab = prefab;
                Weight = weight;
                NeedPartner = needPartner;
                NeedPartnerPrefab = needPartnerPrefab;
                LevelUpChance = levelUpChance;
                SpawnTamed = spawnTamed;
            }
        }

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Procreation m_procreation = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private BaseAITrait m_baseAITrait = null;
        [NonSerialized] private float m_basePregnancyDuration = 60;
        [NonSerialized] private float m_realPregnancyDuration = 0;
        [NonSerialized] private float m_basePregnancyChance = 60;
        [NonSerialized] private float m_realPregnancyChance = 0;
        [NonSerialized] private GameObject m_myPrefab = null;

        // set in registration
        [SerializeField] public bool m_procreateWhileSwimming = true;









        internal static readonly IndexedDataStore<ProcreationPartner[]> s_partnerListStore = new IndexedDataStore<ProcreationPartner[]>();
        [SerializeField] internal int m_partnerListStoreIndex = -1;
        [NonSerialized] public ProcreationPartner[] m_partnerList = null;







        internal static readonly IndexedDataStore<ProcreationOffspring[]> s_offspringListStore = new IndexedDataStore<ProcreationOffspring[]>();
        [SerializeField] internal int m_offspringListStoreIndex = -1;
        [NonSerialized] public ProcreationOffspring[] m_offspringList = null;







        internal static readonly IndexedDataStore<GameObject[]> s_maxCreaturesPrefabsStore = new IndexedDataStore<GameObject[]>();
        [SerializeField] internal int m_maxCreaturesPrefabsStoreIndex = -1;
        [NonSerialized] public GameObject[] m_maxCreaturesPrefabs = null;










        // used for procreation
        [NonSerialized] private GameObject _m_partnerPrefab = null;
        [NonSerialized] private GameObject _m_offspringPrefab = null;
        [NonSerialized] private float _m_offspringLevelUpChance = 0;
        [NonSerialized] private bool _m_offspringNeedPartner = true;
        [NonSerialized] private bool _m_offspringTamed = true;





        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_procreation = GetComponent<Procreation>();
            m_tameable = GetComponent<Tameable>();
            m_character = GetComponent<Character>();
            m_baseAITrait = GetComponent<BaseAITrait>();

            m_basePregnancyDuration = m_procreation.m_pregnancyDuration;
            m_realPregnancyDuration = m_procreation.m_pregnancyDuration;
            m_basePregnancyChance = m_procreation.m_pregnancyChance;
            m_realPregnancyChance = m_procreation.m_pregnancyChance;

            if (m_nview.IsValid())
            {
                m_myPrefab = ZNetScene.instance.GetPrefab(m_nview.GetZDO().GetPrefab());
            }

            s_partnerListStore.TryGet(m_partnerListStoreIndex, out m_partnerList);
            s_offspringListStore.TryGet(m_offspringListStoreIndex, out m_offspringList);

            UpdatePregnancyDuration();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public void SetRealPregnancyDuration(float duration)
        {
            m_realPregnancyDuration = duration;
        }

        public float GetBasePregnancyDuration()
        {
            return m_basePregnancyDuration;
        }

        public float GetRealPregnancyDuration()
        {
            return m_realPregnancyDuration;
        }

        public void SetRealPregnancyChance(float duration)
        {
            m_realPregnancyChance = duration;
        }

        public float GetBasePregnancyChance()
        {
            return m_basePregnancyChance;
        }

        public float GetRealPregnancyChance()
        {
            return m_realPregnancyChance;
        }

        public void UpdatePregnancyDuration()
        {
            if (!m_nview || !m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalPregnancyDurationFactor.Value;
            UpdatePregnancyDuration(globalFactor);
        }

        private void UpdatePregnancyDuration(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_procreation.m_pregnancyDuration = GetBasePregnancyDuration() * totalFactor;
            }
        }

        public string GetProcreationHoverText()
        {
            string text;
            if (m_procreation.IsPregnant())
            {
                text = GetPregnancyLine();
            }
            else
            {
                text = GetLovePointsLine();
            }
            return text;
        }

        private string GetPregnancyLine()
        {
            var zdo = m_nview.GetZDO();
            var zTime = ZNet.instance.GetTime();
            long pregnantLong = zdo.GetLong(ZDOVars.s_pregnant, 0L);
            var dateTime = new DateTime(pregnantLong);
            var duration = GetRealPregnancyDuration();
            double secLeft = duration - (zTime - dateTime).TotalSeconds;

            return Utilities.StringUtils.FormatRelativeTime(
                secLeft,
                labelPositive: "$otab_hover_pregnancy_due",
                labelPositiveAlt: "$otab_hover_pregnancy_due_alt",
                labelNegative: "$otab_hover_pregnancy_overdue",
                labelNegativeAlt: "$otab_hover_pregnancy_overdue_alt",
                colorPositive: Plugin.Configs.HoverColorGood.Value,
                colorNegative: Plugin.Configs.HoverColorBad.Value
            );
        }

        private string GetLovePointsLine()
        {
            if (!Plugin.Configs.HoverShowLovePoints.Value)
            {
                return "";
            }

            if (m_procreation.m_requiredLovePoints == 0)
            {
                return "";
            }

            int lPoints = m_procreation.GetLovePoints();

            var color = lPoints > 0
                ? Plugin.Configs.HoverColorGood.Value
                : Plugin.Configs.HoverColorBad.Value;

            return Localization.instance.Localize(
                "$otab_hover_love_points",
                color,
                lPoints.ToString(),
                m_procreation.m_requiredLovePoints.ToString()
            );
        }

        public string GetAdminHoverInfoText()
        {
            if (!m_nview.IsValid())
            {
                return "";
            }

            var totalInRange = 0;
            var partnersInRange = 0;
            var myPosition = transform.position;

            float m_totalCheckRange = m_procreation.m_totalCheckRange;

            if (_m_partnerPrefab)
            {
                partnersInRange = GetNearbyCountExcludeMyself(_m_partnerPrefab, myPosition, m_procreation.m_partnerCheckRange);
            }
            if (m_maxCreaturesPrefabs != null && m_maxCreaturesPrefabs.Length > 0)
            {
                foreach (var prefab in m_maxCreaturesPrefabs)
                {
                    totalInRange += SpawnSystem.GetNrOfInstances(prefab, myPosition, m_totalCheckRange);
                }
            }
            else
            {
                if (_m_partnerPrefab)
                {
                    totalInRange += SpawnSystem.GetNrOfInstances(_m_partnerPrefab, myPosition, m_totalCheckRange);
                }
                if (_m_offspringPrefab)
                {
                    totalInRange += SpawnSystem.GetNrOfInstances(_m_offspringPrefab, myPosition, m_totalCheckRange);
                }
            }

            var text = "";
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Pregnancy chance: " + (int)((1 - m_realPregnancyChance) * 100) + "%");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Partner prefab: " + (_m_partnerPrefab?.gameObject.name ?? "null"));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Partners: " + partnersInRange + " within " + m_procreation.m_partnerCheckRange + " meters");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Total: " + totalInRange + " within " + m_totalCheckRange + " meters");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring prefab: " + (_m_offspringPrefab?.gameObject.name ?? "null"));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Need Partner: " + (_m_offspringNeedPartner ? "true" : "false"));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring level up chance: " + (int)(_m_offspringLevelUpChance * 100));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring tamed: " + (_m_offspringTamed ? "true" : "false"));
            return text;
        }
        





        private int GetNearbyCountExcludeMyself(GameObject ofPrefab, Vector3 position, float range)
        {
            if (range <= 0)
            {
                return 0;
            }

            var count = SpawnSystem.GetNrOfInstances(
                ofPrefab,
                position,
                range,
                eventCreaturesOnly: false,
                procreationOnly: true);

            if (ofPrefab == m_myPrefab)
            {
                count -= 1;
            }
            return count;
        }


        internal void OnProcreate()
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return;
            }

            // Procreation component COULD work without Tameable component
            // maybe one day I gonna add non-tameable procration feature
            // but that would also mean to build a feeding/hungry workaround - maybe in far future
            var isTamed = m_tameable ? m_tameable.IsTamed() : (m_character ? m_character.IsTamed() : false);
            if (!isTamed)
            {
                return;
            }

            if (m_procreateWhileSwimming == false && m_character && m_character.IsSwimming())
            {
                return;
            }

            if (Plugin.Configs.PreventProcreationWhileFollowing.Value == true && m_baseAITrait.GetFollowTarget())
            {
                return;
            }

            var zdo = m_nview.GetZDO();

            // original block, just keep it
            if (m_procreation.GetMyPrefab() == null)
            {
                m_procreation.SetMyPrefab(m_myPrefab);
            }

            // "c_" => pseudo constants - do not change the values!
            var c_myPosition = m_procreation.transform.position;
            var c_myPartnerCheckRange = m_procreation.m_partnerCheckRange;
            var c_myTotalCheckRange = m_procreation.m_totalCheckRange;
            var c_myPrefabName = Utils.GetPrefabName(m_myPrefab.name);
            var c_nowTicks = ZNet.instance.GetTime().Ticks;
            var c_zNetScene = ZNetScene.instance;
            bool c_isPregnant = m_procreation.IsPregnant();
            bool c_isDue = m_procreation.IsDue();
            
            // s_ => valheim zdo vars
            int s_lovePoints = zdo.GetInt(ZDOVars.s_lovePoints);

            // z_ => zdo values, change via ZNetHelper
            string z_partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "");

            if (!c_isPregnant)
            {
                if (UnityEngine.Random.value <= m_procreation.m_pregnancyChance)
                {
                    // warning: m_pregnancyChance is actually the failure chance;
                    // Valheim uses inverted logic here
                    return;
                }
                if (m_baseAITrait.IsAlerted() || m_baseAITrait.IsConfined() || m_tameable.IsHungry())
                {
                    return;
                }

                _m_partnerPrefab = null;
                _m_offspringPrefab = null;

                if (m_partnerList != null && m_partnerList.Length > 0)
                {
                    var foundPartner = Common.WeightedRandom.FindRandom(m_partnerList, out ProcreationPartner partnerEntry, entry =>
                    {
                        var prefab = c_zNetScene.GetPrefab(entry.Prefab);
                        if (prefab == null) return 0; // zero weight => skip this one
                        return entry.Weight * GetNearbyCountExcludeMyself(prefab, c_myPosition, c_myPartnerCheckRange);
                    });
                    if (foundPartner)
                    {
                        _m_partnerPrefab = c_zNetScene.GetPrefab(partnerEntry.Prefab);
                    }
                }
            }

            if (c_isPregnant && !_m_partnerPrefab)
            {
                if (z_partnerPrefab.Length != 0)
                {
                    GameObject prefab = c_zNetScene.GetPrefab(z_partnerPrefab);
                    if (prefab)
                    {
                        _m_partnerPrefab = prefab;
                    }
                }
                if (!_m_partnerPrefab)
                {
                    m_procreation.ResetPregnancy();
                    s_lovePoints = ZDOUtils.SetInt(zdo, ZDOVars.s_lovePoints, 0);
                    c_isPregnant = m_procreation.IsPregnant();
                    c_isDue = m_procreation.IsDue();
                    z_partnerPrefab = ZDOUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                }
            }

            if (!_m_offspringPrefab && m_offspringList  != null && m_offspringList.Length > 0)
            {
                _m_offspringTamed = true;
                _m_offspringNeedPartner = true;
                _m_offspringLevelUpChance = 0f;

                var flag1 = (bool)_m_partnerPrefab;
                var foundOffspring = Common.WeightedRandom.FindRandom(m_offspringList, out var randomOffspring, entry =>
                {
                    var validPartner = entry.NeedPartner == false || (flag1 && (string.IsNullOrEmpty(entry.NeedPartnerPrefab) || _m_partnerPrefab.name == entry.NeedPartnerPrefab));
                    return validPartner ? entry.Weight : 0;
                });
                if (foundOffspring)
                {
                    _m_offspringPrefab = c_zNetScene.GetPrefab(randomOffspring.Prefab);
                    _m_offspringTamed = randomOffspring.SpawnTamed;
                    _m_offspringNeedPartner = randomOffspring.NeedPartner;
                    _m_offspringLevelUpChance = randomOffspring.LevelUpChance;
                }
            }

            // no offspring -> no procreation
            if (!_m_offspringPrefab)
            {
                return;
            }

            // handle self breed procreation
            if (!_m_offspringNeedPartner && _m_partnerPrefab != m_myPrefab)
            {
                _m_partnerPrefab = m_myPrefab; // we are targeting ourself as partner for the next procreation 
            }

            // no partner found in the end?
            if (!_m_partnerPrefab)
            {
                return;
            }

            if (!c_isPregnant)
            {
                // check max creatures in range
                int maxCreaturesLeft = m_procreation.m_maxCreatures;
                if (maxCreaturesLeft > 0)
                {
                    if (m_maxCreaturesPrefabs != null)
                    {
                        if (m_maxCreaturesPrefabs.Length > 0)
                        {
                            foreach (var prefab in m_maxCreaturesPrefabs)
                            {
                                maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(prefab, c_myPosition, c_myTotalCheckRange);
                                if (maxCreaturesLeft <= 0)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(_m_partnerPrefab, c_myPosition, c_myTotalCheckRange);
                        if (maxCreaturesLeft <= 0)
                        {
                            return;
                        }
                        maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(_m_offspringPrefab, c_myPosition, c_myTotalCheckRange);
                        if (maxCreaturesLeft <= 0)
                        {
                            return;
                        }
                    }
                }

                if (_m_offspringNeedPartner)
                {
                    // only show for procreation with partner
                    //m_nview.InvokeRPC(ZNetView.Everybody, "RPC_DisplayLoveEffect");
                    m_procreation.m_loveEffects?.Create(m_procreation.transform.position, m_procreation.transform.rotation);
                }

                s_lovePoints++;
                if (s_lovePoints >= m_procreation.m_requiredLovePoints)
                {
                    m_procreation.MakePregnant();
                    s_lovePoints = 0;
                    c_isPregnant = m_procreation.IsPregnant();
                    c_isDue = m_procreation.IsDue();
                    z_partnerPrefab = ZDOUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, _m_partnerPrefab.name, z_partnerPrefab);
                }
                s_lovePoints = ZDOUtils.SetInt(zdo, ZDOVars.s_lovePoints, s_lovePoints);
            }
            
            // dont use elseif. we allow giving birth right after getting pregnant
            if (c_isDue)
            {
                m_procreation.ResetPregnancy();

                Vector3 forward = m_procreation.transform.forward;
                Vector3 dir = forward;
                if (m_procreation.m_spawnRandomDirection)
                {
                    float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                    dir = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
                }

                float offset = (m_procreation.m_spawnOffsetMax > 0f)
                    ? UnityEngine.Random.Range(m_procreation.m_spawnOffset, m_procreation.m_spawnOffsetMax)
                    : m_procreation.m_spawnOffset;

                GameObject spawned = UnityEngine.Object.Instantiate(
                    _m_offspringPrefab,
                    c_myPosition - dir * offset,
                    Quaternion.LookRotation(-forward, Vector3.up));

                var level = Mathf.Max(m_procreation.m_minOffspringLevel, m_character ? m_character.GetLevel() : m_procreation.m_minOffspringLevel);
                var levelUp = UnityEngine.Random.value < (Plugin.Configs.GlobalBaseLevelUpChance.Value + _m_offspringLevelUpChance);

                Character spawnedCharacter = spawned.GetComponent<Character>();
                if (spawnedCharacter != null)
                {

                    if (levelUp)
                    {
                        var spawnedCharacterTrait = spawned.GetComponent<CharacterTrait>();
                        if (spawnedCharacterTrait && spawnedCharacterTrait.m_maxLevel > 0)
                        {
                            if (level < spawnedCharacterTrait.m_maxLevel)
                            {
                                level += 1;
                            }
                        }
                        else
                        {
                            // important todo: warning, cannot levelup
                        }
                    }

                    spawnedCharacter.SetTamed(_m_offspringTamed);
                    spawnedCharacter.SetLevel(level);
                }
                else
                {
                    var spawnedItemDrop = spawned.GetComponent<ItemDrop>();
                    if (spawnedItemDrop)
                    {
                        if (levelUp && level < spawnedItemDrop.m_itemData.m_shared.m_maxQuality)
                        {
                            level += 1;
                        }
                        spawnedItemDrop.SetQuality(level);
                    }
                    else
                    {
                        // important todo: warning, cannot set level
                    }
                }

                //m_nview.InvokeRPC(ZNetView.Everybody, "RPC_DisplayBirthEffect", spawned.transform.position);
                m_procreation.m_birthEffects.Create(spawned.transform.position, Quaternion.identity);

                // CLLC traits (it also takes care if the spawned object is an egg or growup)
                Integrations.Mods.CllCBridge.BequeathTraits(m_nview.GetComponent<Character>(), _m_partnerPrefab, spawned);

                // todo: problem:
                // what happens when an egg gets stacked?
                // Egg1 with trait1 gets stacked into Egg2 with trait2. Egg1 (and also trait1) gets destroyed
                // Result: We now got to eggs (Egg2 with stacksize of 2) with trait2
                // when does an item can get stacked?
                // - user pick up item in invetory and drops on other item with same type
                // - valheims auto stacking of dropped items
                // - an other mod could just say: remove item 1 and set stacksize of item 2 to +1
                // therefore: we cannot garuantee to pass correct traits over to eggs
                // but wait... it that a problem otab should care about? cllc should handle it itself

                // reset
                _m_partnerPrefab = null;
                _m_offspringPrefab = null;
                _m_offspringNeedPartner = true;
                _m_offspringTamed = true;
                _m_offspringLevelUpChance = 0f;
                z_partnerPrefab = ZDOUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);

            }

        }

    }
}
