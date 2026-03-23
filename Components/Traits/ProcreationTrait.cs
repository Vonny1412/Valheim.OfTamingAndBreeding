using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.Components.Extensions;
using OfTamingAndBreeding.OTABUtils;
using OfTamingAndBreeding.ValheimAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Traits
{
    public class ProcreationTrait : OTABComponent<ProcreationTrait>
    {
        public class ProcreationPartner : Common.WeightedRandom.IWeighted
        {
            public float Weight { get; }
            public string Prefab { get; }
            public ProcreationPartner(
                string prefab,
                float weight
                )
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
            public int MaxLevel { get; }
            public bool SpawnTamed { get; }
            public ProcreationOffspring(
                string prefab,
                float weight,
                bool needPartner,
                string needPartnerPrefab,
                float levelUpChance,
                int maxLevel,
                bool spawnTamed
            )
            {
                Prefab = prefab;
                Weight = weight;
                NeedPartner = needPartner;
                NeedPartnerPrefab = needPartnerPrefab;
                LevelUpChance = levelUpChance;
                MaxLevel = maxLevel;
                SpawnTamed = spawnTamed;
            }
        }

        private static readonly List<ProcreationPartner[]> _partnerData;
        private static readonly List<ProcreationOffspring[]> _offspringData;
        private static readonly List<string[]> _maxCreaturesPrefabs;

        static ProcreationTrait()
        {
            _partnerData = new List<ProcreationPartner[]>();
            _offspringData = new List<ProcreationOffspring[]>();
            _maxCreaturesPrefabs = new List<string[]>();

            Net.NetworkSessionManager.Instance.OnSessionClosed += (netsess, dataLoaded) => {
                _partnerData.Clear();
                _offspringData.Clear();
                _maxCreaturesPrefabs.Clear();
            };
        }

        // set in awake
        [NonSerialized] private ZNetView m_nview = null;
        [NonSerialized] private Procreation m_procreation = null;
        [NonSerialized] private Tameable m_tameable = null;
        [NonSerialized] private Character m_character = null;
        [NonSerialized] private BaseAI m_baseAI = null;
        [NonSerialized] private float m_basePregnancyDuration = 60;
        [NonSerialized] private float m_realPregnancyDuration = 0;
        [NonSerialized] private GameObject m_myPrefab = null;

        // set in registration
        [SerializeField] public long m_partnerRecheckTicks = 0;
        [SerializeField] public bool m_procreateWhileSwimming = true;
        [SerializeField] public int m_maxSiblingsPerPregnancy = 0;
        [SerializeField] public float m_extraSiblingChance = 0;
        [SerializeField] private int m_partnerListIndex = -1;
        [SerializeField] private int m_offspringListIndex = -1;
        [SerializeField] private int m_maxCreaturesPrefabsIndex = -1;

        // used for procreation
        [NonSerialized] private GameObject m_partnerPrefab = null;
        [NonSerialized] private GameObject m_offspringPrefab = null;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_procreation = GetComponent<Procreation>();
            m_tameable = GetComponent<Tameable>();
            m_character = GetComponent<Character>();
            m_baseAI = GetComponent<BaseAI>();

            m_basePregnancyDuration = m_procreation.m_pregnancyDuration;
            m_realPregnancyDuration = m_procreation.m_pregnancyDuration;

            if (m_nview.IsValid())
            {
                // sadly we need to wrap the target methods because valheim is doing this:
                // > m_action.DynamicInvoke(ZNetView.Deserialize(rpc, m_action.Method.GetParameters(), pkg));
                // the first param of the extension methods (this Procreation procreation) is making problems while deserializing
                m_nview.Register("RPC_DisplayLoveEffect", (long sender) => RPC_DisplayLoveEffect(sender));
                m_nview.Register<Vector3>("RPC_DisplayBirthEffect", (long sender, Vector3 position) => RPC_DisplayBirthEffect(sender, position));

                m_myPrefab = ZNetScene.instance.GetPrefab(m_nview.GetZDO().GetPrefab());
            }

            UpdatePregnancyDuration();

            Register(this);
        }

        private void OnDestroy()
        {
            Unregister(this);
        }

        public void SetPartnerList(ProcreationPartner[] partnerList)
        {
            m_partnerListIndex = _partnerData.Count;
            _partnerData.Add(partnerList);
        }

        public bool HasPartnerList(out ProcreationPartner[] partnerList)
        {
            if (m_partnerListIndex != -1)
            {
                partnerList = _partnerData[m_partnerListIndex];
                return true;
            }
            partnerList = null;
            return false;
        }

        public void SetOffspringList(ProcreationOffspring[] offspringList)
        {
            m_offspringListIndex = _offspringData.Count;
            _offspringData.Add(offspringList);
        }

        public bool HasOffspringList(out ProcreationOffspring[] offspringList)
        {
            if (m_offspringListIndex != -1)
            {
                offspringList = _offspringData[m_offspringListIndex];
                return true;
            }
            offspringList = null;
            return false;
        }

        public void SetMaxCreaturesPrefabs(string[] prefabNames)
        {
            m_maxCreaturesPrefabsIndex = _maxCreaturesPrefabs.Count;
            _maxCreaturesPrefabs.Add(prefabNames);
        }

        public bool HasMaxCreaturesPrefabs(out string[] prefabNames)
        {
            if (m_maxCreaturesPrefabsIndex != -1)
            {
                prefabNames = _maxCreaturesPrefabs[m_maxCreaturesPrefabsIndex];
                return true;
            }
            prefabNames = null;
            return false;
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

        public void UpdatePregnancyDuration()
        {
            if (!m_nview || !m_nview.IsValid()) return;

            var globalFactor = Plugin.Configs.GlobalPregnancyDurationFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //procreation.UpdatePregnancyDuration(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            UpdatePregnancyDuration(totalFactor);
        }

        private void UpdatePregnancyDuration(float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                m_procreation.m_pregnancyDuration = GetBasePregnancyDuration() * totalFactor;
            }
        }

        private void RPC_DisplayLoveEffect(long sender)
        {
            if (!m_procreation.gameObject) return;
            m_procreation.m_loveEffects?.Create(m_procreation.transform.position, m_procreation.transform.rotation);
        }

        private void RPC_DisplayBirthEffect(long sender, Vector3 position)
        {
            if (!m_procreation.gameObject) return;
            m_procreation.m_birthEffects?.Create(position, Quaternion.identity);
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

            return OTABUtils.StringUtils.FormatRelativeTime(
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

            var zdo = m_nview.GetZDO();
            var text = "";
            var ticks = ZNet.instance.GetTime().Ticks;

            string partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "-");
            bool partnerFound = m_partnerPrefab != null;

            long partnerNotSeenSince = zdo.GetLong(Plugin.ZDOVars.z_partnerNotSeenSince, 0L);
            bool partnerExpired = partnerNotSeenSince != 0L && (ticks - partnerNotSeenSince) > m_partnerRecheckTicks;

            double partnerExpiredSeconds = 0;
            if (partnerExpired)
            {
                long expireTicks = partnerNotSeenSince + m_partnerRecheckTicks;
                partnerExpiredSeconds = (ticks - expireTicks) / 10000000.0;
            }

            string offspringPrefab = zdo.GetString(Plugin.ZDOVars.z_offspringPrefab, "");
            int siblingsCounter = zdo.GetInt(Plugin.ZDOVars.z_siblingsCounter, 0);

            int offspringLevel = zdo.GetInt(Plugin.ZDOVars.z_offspringLevel, m_procreation.m_minOffspringLevel);
            bool offspringTamed = zdo.GetInt(Plugin.ZDOVars.z_offspringTamed, 1) == 1;
            bool needPartner = zdo.GetInt(Plugin.ZDOVars.z_needPartner, 1) == 1;
            int siblingChance = (int)(m_extraSiblingChance * 100);

            var totalInRange = 0;
            var partnersInRange = 0;
            var myPosition = transform.position;

            if (m_partnerPrefab)
            {
                partnersInRange = GetNearbyCountExcludeMyself(m_partnerPrefab, myPosition, m_procreation.m_partnerCheckRange);
            }
            if (HasMaxCreaturesPrefabs(out var prefabNames))
            {
                foreach (var prefabName in prefabNames)
                {
                    totalInRange += SpawnSystem.GetNrOfInstances(ZNetScene.instance.GetPrefab(prefabName), myPosition, m_procreation.m_totalCheckRange);
                }
            }
            else
            {
                totalInRange += SpawnSystem.GetNrOfInstances(m_partnerPrefab, myPosition, m_procreation.m_totalCheckRange);
                totalInRange += SpawnSystem.GetNrOfInstances(m_offspringPrefab, myPosition, m_procreation.m_totalCheckRange);
            }

            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Partner prefab: " + partnerPrefab + " (found:"+ (partnerFound ? "true" : "false") + ")");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Not seen since: " + (int)partnerExpiredSeconds + " seconds");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Partners: " + partnersInRange + " within " + m_procreation.m_partnerCheckRange + " meters");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Total: " + totalInRange + " within " + m_procreation.m_totalCheckRange + " meters");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring prefab: " + offspringPrefab);
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Sibling Chance: " + siblingChance + "%" + " ("+ siblingsCounter + "/" + m_maxSiblingsPerPregnancy + ")");
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Need Partner: " + (needPartner ? "true" : "false"));
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring level: " + offspringLevel);
            text += "\n" + Localization.instance.Localize("$otab_hover_admin_info", "Offspring tamed: " + (offspringTamed ? "true" : "false"));

            return text;
        }
        
        public bool OnProcreate()
        {
            // Procreation component COULD work without Tameable component
            // maybe one day I gonna add non-tameable procration feature
            // but that would also mean to build a feeding/hungry workaround - maybe in far future
            var __isTamed = m_tameable ? m_tameable.IsTamed() : (m_character ? m_character.IsTamed() : false);

            if (!m_nview.IsValid() || !m_nview.IsOwner() || !__isTamed)
            {
                // note: valheim also immediatly returns if its not the owner
                return true; // return as handled
            }

            // check if procreation is disabled while swimming
            if (m_procreateWhileSwimming == false && m_character && m_character.IsSwimming())
            {
                return true; // return as handled
            }

            DoProcreate(); // code block separated because from now on we only return "true"
            return true; // we did the job
        }





        private int GetNearbyCountExcludeMyself(GameObject ofPrefab, Vector3 position, float range)
        {
            var count = SpawnSystem.GetNrOfInstances(
                ofPrefab, position, range,
                eventCreaturesOnly: false,
                procreationOnly: true);
            if (ofPrefab == m_myPrefab)
            {
                count -= 1;
            }
            return count;
        }




        private void DoProcreate()
        {
            var zdo = m_nview.GetZDO();

            // original block, just keep it
            if (m_procreation.GetMyPrefab() == null)
            {
                m_procreation.SetMyPrefab(m_myPrefab);
            }

            // "__" => pseudo constants - do not change the values!
            var __myPosition = m_procreation.transform.position;
            var __myPartnerCheckRange = m_procreation.m_partnerCheckRange;
            var __myTotalCheckRange = m_procreation.m_totalCheckRange;
            var __myPrefabName = Utils.GetPrefabName(m_myPrefab.name);
            var __nowTicks = ZNet.instance.GetTime().Ticks;
            var __zNetScene = ZNetScene.instance;
            bool __isPregnant = m_procreation.IsPregnant();
            bool __isDue = m_procreation.IsDue();
            
            // s_ => valheim zdo vars
            int s_lovePoints = zdo.GetInt(ZDOVars.s_lovePoints);

            // z_ => zdo values, change via ZNetHelper
            string z_partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "");
            long z_partnerNotSeenSince = zdo.GetLong(Plugin.ZDOVars.z_partnerNotSeenSince, 0L);
            string z_offspringPrefab = zdo.GetString(Plugin.ZDOVars.z_offspringPrefab, "");
            int z_siblingsCounter = zdo.GetInt(Plugin.ZDOVars.z_siblingsCounter, 0);
            int z_offspringLevel = zdo.GetInt(Plugin.ZDOVars.z_offspringLevel, m_procreation.m_minOffspringLevel);
            int z_needPartner = zdo.GetInt(Plugin.ZDOVars.z_needPartner, 1);
            int z_offspringTamed = zdo.GetInt(Plugin.ZDOVars.z_offspringTamed, 1);

            bool doResetPartner = false;
            bool doResetOffspring = false;
            var searchNewOffspring = false;

            //------------------------------------------------------
            //-- partner restoring
            //------------------------------------------------------

            // partner is stored in zdo
            if (!m_partnerPrefab && z_partnerPrefab.Length != 0)
            {
                var prefab = __zNetScene.GetPrefab(z_partnerPrefab);
                if (prefab)
                {
                    // use value in zdo as partner
                    m_partnerPrefab = prefab;
                }
                else
                {
                    // invalid zdo -> clear
                    z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                }
            }
            // debug: save current selected partner in zdo
            else if (m_partnerPrefab && z_partnerPrefab.Length == 0)
            {
                z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_partnerPrefab.name, z_partnerPrefab);
            }

            //------------------------------------------------------
            //-- partner mourning
            //------------------------------------------------------

            // any partner is selected and we need a partner
            if (m_partnerPrefab && z_needPartner == 1)
            {
                bool keepPartner;
                if (__isPregnant)
                {
                    // always keep while beeing pregnant
                    keepPartner = true;
                }
                else
                {
                    // i am using __myTotalCheckRange instead of __myPartnerCheckRange
                    // so the old partner wont get abendoned if it just stays outside the smaller partner check range for too long
                    int count = GetNearbyCountExcludeMyself(m_partnerPrefab, __myPosition, __myTotalCheckRange);
                    keepPartner = count >= z_needPartner;
                }

                if (keepPartner)
                {
                    // partner still nearby
                    z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                }
                else if (m_partnerRecheckTicks > 0) // this feature can be turned off by setting it to 0 but would result in less pregnancies
                {
                    if (z_partnerNotSeenSince == 0L)
                    {
                        // only set this value if it has not been set yet!
                        z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, __nowTicks, z_partnerNotSeenSince);
                    }
                    else
                    {
                        bool expired = (__nowTicks - z_partnerNotSeenSince) > m_partnerRecheckTicks;
                        if (expired) // creature is mourning - lets find some one new!
                        {
                            // this will trigger a new search for partner
                            z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                            z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                            m_partnerPrefab = null;
                        }
                    }
                }
                
            }

            //------------------------------------------------------
            //-- partner selection
            //------------------------------------------------------

            if (!m_partnerPrefab && HasPartnerList(out var partnerList))
            {
                var foundPartner = Common.WeightedRandom.FindRandom<ProcreationPartner>(partnerList, out ProcreationPartner partnerEntry, entry =>
                {
                    var prefab = __zNetScene.GetPrefab(entry.Prefab);
                    if (prefab == null) return 0; // zero weight => skip this one
                    return entry.Weight * GetNearbyCountExcludeMyself(prefab, __myPosition, __myPartnerCheckRange);
                });
                if (foundPartner)
                {
                    m_partnerPrefab = __zNetScene.GetPrefab(partnerEntry.Prefab);
                    z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_partnerPrefab.name, z_partnerPrefab);
                    z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                    searchNewOffspring = true;
                }
            }

            //------------------------------------------------------
            //-- offspring restoring
            //------------------------------------------------------

            // try load offspring from zdo
            if (!searchNewOffspring) // but only if we are not gonna search for new offspring anyway
            {
                if (!m_offspringPrefab && z_offspringPrefab.Length != 0)
                {
                    var prefab = __zNetScene.GetPrefab(z_offspringPrefab);
                    if (prefab)
                    {
                        m_offspringPrefab = prefab;
                        m_procreation.SetOffspringPrefab(m_offspringPrefab);
                    }
                    else
                    {
                        z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                    }
                }
                // debug: save current selected offspring in zdo
                else if (m_offspringPrefab && z_offspringPrefab.Length == 0)
                {
                    z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, m_offspringPrefab.name, z_offspringPrefab);
                }
            }

            //------------------------------------------------------
            //-- offspring selection
            //------------------------------------------------------

            if ((searchNewOffspring || !m_offspringPrefab) && HasOffspringList(out var offspringList))
            {
                var flag1 = (bool)m_partnerPrefab;
                var foundOffspring = Common.WeightedRandom.FindRandom<ProcreationOffspring>(offspringList, out ProcreationOffspring randomOffspring, entry =>
                {
                    var validPartner =
                        entry.NeedPartner == false ||
                        flag1 && (string.IsNullOrEmpty(entry.NeedPartnerPrefab) || m_partnerPrefab.name == entry.NeedPartnerPrefab);
                    return validPartner ? entry.Weight : 0;
                });

                if (foundOffspring)
                {
                    var offspring = __zNetScene.GetPrefab(randomOffspring.Prefab);

                    z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, offspring.name, z_offspringPrefab);
                    m_offspringPrefab = offspring;

                    var val1 = randomOffspring.SpawnTamed ? 1 : 0;
                    z_offspringTamed = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringTamed, val1, z_offspringTamed);

                    var val2 = randomOffspring.NeedPartner ? 1 : 0;
                    z_needPartner = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_needPartner, val2, z_needPartner);

                    var levelOld = Mathf.Max(m_procreation.m_minOffspringLevel, m_character ? m_character.GetLevel() : m_procreation.m_minOffspringLevel);
                    var levelNew = levelOld;

                    // determ offspring level for levelup feature
                    // this is not the best place to set level of next offspring
                    // because the fate will be determined before creature gets pregnant
                    // but i want to store neither offspring-entry-index nor levelup-values into zdo
                    // so just roll the dice here...
                    if (randomOffspring.MaxLevel > levelOld && UnityEngine.Random.value < randomOffspring.LevelUpChance)
                    {
                        levelNew = levelNew + 1;
                        Plugin.LogServerDebug($"Offspring '{offspring.name}' level up: {levelOld} -> {levelNew}");
                    }

                    z_offspringLevel = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, levelNew, z_offspringLevel);
                }
            }

            //------------------------------------------------------
            //-- validation
            //------------------------------------------------------

            // no offspring -> no procreation
            if (m_offspringPrefab == null)
            {
                z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, 0, z_siblingsCounter);
                return;
            }

            // handle no-partner procreation
            if (z_needPartner == 0 && m_partnerPrefab != m_myPrefab)
            {
                m_partnerPrefab = m_myPrefab; // we are targeting ourself as partner for the next procreation 
                z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_partnerPrefab.name, z_partnerPrefab);
            }

            // no partner found in the end?
            if (m_partnerPrefab == null)
            {
                return;
            }

            //------------------------------------------------------
            //-- procreation
            //------------------------------------------------------

            if (!__isPregnant)
            {
                bool canProcreate = UnityEngine.Random.value <= m_procreation.m_pregnancyChance && !m_baseAI.IsAlerted() && !m_tameable.IsHungry();
                if (canProcreate)
                {
                    // max creatures check: only using m_partnerPrefab + offspring (never m_myPrefab)
                    int maxCreaturesLeft = m_procreation.m_maxCreatures;
                    if (HasMaxCreaturesPrefabs(out var prefabNames))
                    {
                        foreach (var prefabName in prefabNames)
                        {
                            maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(__zNetScene.GetPrefab(prefabName), __myPosition, __myTotalCheckRange);
                            if (maxCreaturesLeft <= 0) return; // early return
                        }
                    }
                    else
                    {
                        // partners
                        maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(m_partnerPrefab, __myPosition, __myTotalCheckRange);
                        if (maxCreaturesLeft <= 0) return; // early return
                        // offsprings
                        maxCreaturesLeft -= SpawnSystem.GetNrOfInstances(m_offspringPrefab, __myPosition, __myTotalCheckRange);
                        if (maxCreaturesLeft <= 0) return; // early return
                    }

                    int partnersInRange = GetNearbyCountExcludeMyself(m_partnerPrefab, __myPosition, __myPartnerCheckRange);
                    if (partnersInRange >= z_needPartner)
                    {
                        if (z_needPartner == 1)
                        {
                            // only show for procreation with partner
                            //m_loveEffects.Create(__myPosition, transform.rotation);
                            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_DisplayLoveEffect");
                        }

                        s_lovePoints++;
                        if (s_lovePoints >= m_procreation.m_requiredLovePoints)
                        {
                            m_procreation.MakePregnant();
                            s_lovePoints = 0;
                            __isPregnant = m_procreation.IsPregnant();
                            __isDue = m_procreation.IsDue();
                        }
                        s_lovePoints = ZNetUtils.SetInt(zdo, ZDOVars.s_lovePoints, s_lovePoints);
                    }
                }
            }

            if (__isDue)
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
                    m_offspringPrefab,
                    __myPosition - dir * offset,
                    Quaternion.LookRotation(-forward, Vector3.up));

                Character spawnedCharacter = spawned.GetComponent<Character>();
                int level = Mathf.Max(m_procreation.m_minOffspringLevel, z_offspringLevel);
                if (spawnedCharacter != null)
                {
                    spawnedCharacter.SetTamed(z_offspringTamed == 1);
                    spawnedCharacter.SetLevel(level);
                }
                else
                {
                    spawned.GetComponent<ItemDrop>()?.SetQuality(level);
                }

                //m_birthEffects.Create(spawned.transform.position, Quaternion.identity);
                m_nview.InvokeRPC(ZNetView.Everybody, "RPC_DisplayBirthEffect", spawned.transform.position);

                // CLLC traits (it also takes care if the spawned object is an egg or growup)
                ThirdParty.Mods.CllCBridge.BequeathTraits(m_nview.GetComponent<Character>(), m_partnerPrefab, spawned);

                //---------------------------------
                // sibling handling
                //---------------------------------

                bool unlimited = m_maxSiblingsPerPregnancy < 0; // -1
                bool canHaveMore = unlimited || z_siblingsCounter < m_maxSiblingsPerPregnancy;
                if (canHaveMore && UnityEngine.Random.value <= m_extraSiblingChance)
                {
                    ZNetUtils.SetLong(zdo, ZDOVars.s_pregnant, __nowTicks - TimeSpan.FromSeconds(m_procreation.m_pregnancyDuration).Ticks);
                    z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, z_siblingsCounter + 1);
                }
                else
                {
                    doResetPartner = true;
                }
                doResetOffspring = true;
            }

            //------------------------------------------------------
            //-- reset
            //------------------------------------------------------

            if (doResetPartner)
            {
                z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, 0, z_siblingsCounter);
                m_partnerPrefab = null;
                doResetOffspring = true;
            }

            if (doResetOffspring)
            {
                z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                z_offspringLevel = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, m_procreation.m_minOffspringLevel, z_offspringLevel);
                z_needPartner = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 1, z_needPartner);
                m_offspringPrefab = null;
            }

        }

    }
}
