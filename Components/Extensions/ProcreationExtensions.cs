using Jotunn.Managers;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ProcreationExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ZNetView GetZNetView(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_nview_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tameable GetTameable(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_tameable_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Character GetCharacter(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_character_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetMyPrefab(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_myPrefab_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMyPrefab(this Procreation that, GameObject prefab)
            => ValheimAPI.Procreation.__IAPI_m_myPrefab_Invoker.Set(that, prefab);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPregnant(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_IsPregnant_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDue(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_IsDue_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetOffspringPrefab(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_offspringPrefab_Invoker.Get(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOffspringPrefab(this Procreation that, GameObject prefab)
            => ValheimAPI.Procreation.__IAPI_m_offspringPrefab_Invoker.Set(that, prefab);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakePregnant(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_MakePregnant_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetPregnancy(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_ResetPregnancy_Invoker1.Invoke(that);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BaseAI GetBaseAI(this Procreation that)
            => ValheimAPI.Procreation.__IAPI_m_baseAI_Invoker.Get(that);

        private static void RPC_DisplayLoveEffect(this Procreation procreation, long sender)
        {
            if (!procreation.gameObject) return;
            procreation.m_loveEffects?.Create(procreation.transform.position, procreation.transform.rotation);
        }

        private static void RPC_DisplayBirthEffect(this Procreation procreation, long sender, Vector3 position)
        {
            if (!procreation.gameObject) return;
            procreation.m_birthEffects?.Create(position, Quaternion.identity);
        }

        public static void Awake_PatchPostfix(this Procreation procreation)
        {
            var m_nview = procreation.GetZNetView();
            if (m_nview.IsValid())
            {
                // sadly we need to wrap the target methods because valheim is doing this:
                // > m_action.DynamicInvoke(ZNetView.Deserialize(rpc, m_action.Method.GetParameters(), pkg));
                // the first param of the extension methods (this Procreation procreation) is making problems while deserializing
                m_nview.Register("RPC_DisplayLoveEffect", (long sender) => procreation.RPC_DisplayLoveEffect(sender));
                m_nview.Register<Vector3>("RPC_DisplayBirthEffect", (long sender, Vector3 position) => procreation.RPC_DisplayBirthEffect(sender, position));
            }

            if (procreation.TryGetComponent<OTAB_ProcreationTrait>(out _) == false)
            {
                // we are using late-registration
                // instead of adding component in CreatureProcessor

                var prefabName = global::Utils.GetPrefabName(procreation.gameObject.name);
                var prefab = PrefabManager.Instance.GetPrefab(prefabName);
                var prefabProcreation = prefab.GetComponent<Procreation>();

                var c1 = procreation.gameObject.gameObject.AddComponent<OTAB_ProcreationTrait>();
                var c2 = prefab.gameObject.AddComponent<OTAB_ProcreationTrait>();

                c1.m_basePregnancyDuration = prefabProcreation.m_pregnancyDuration;
                c2.m_basePregnancyDuration = prefabProcreation.m_pregnancyDuration;

                c1.m_realPregnancyDuration = prefabProcreation.m_pregnancyDuration;
                c2.m_realPregnancyDuration = prefabProcreation.m_pregnancyDuration;
            }

            procreation.UpdatePregnancyDuration();
        }

        public static void UpdatePregnancyDuration(this Procreation procreation)
        {
            var m_nview = procreation.GetZNetView();
            if (!m_nview || !m_nview.IsValid()) return;
            var zdo = m_nview.GetZDO();
            if (zdo == null) return;

            var globalFactor = Plugin.Configs.GlobalPregnancyDurationFactor.Value;
            if (globalFactor < 0f)
            {
                // should not be possible but whatever
                //procreation.UpdatePregnancyDuration(1f); // back to base
                return;
            }
            var totalFactor = globalFactor;
            procreation.UpdatePregnancyDuration(totalFactor);
        }

        private static void UpdatePregnancyDuration(this Procreation procreation, float totalFactor)
        {
            if (totalFactor >= 0) // yes, we do allow 0, too
            {
                var trait = procreation.GetComponent<OTAB_ProcreationTrait>();
                procreation.m_pregnancyDuration = trait.m_basePregnancyDuration * totalFactor;
            }
        }

        public static bool Procreate_PatchPrefix(this Procreation procreation)
        {
            var m_tameable = procreation.GetTameable();
            var m_character = procreation.GetCharacter();
            var m_nview = procreation.GetZNetView();

            // why doing this? because Procreation component COULD work without Tameable component
            // maybe one day I gonna add non-tameable procration feature
            // but that would also mean to build a feeding/hungry workaround 3- maybe in far future
            var __isTamed = m_tameable ? m_tameable.IsTamed() : (m_character ? m_character.IsTamed() : false);

            if (!m_nview.IsValid() || !m_nview.IsOwner() || !__isTamed)
            {
                // note: valheim also immediatly returns if its not the owner
                return false;
            }
            var zdo = m_nview.GetZDO();

            var m_myPrefab = procreation.GetMyPrefab();

            // original block, just keep it
            // m_myPrefab will nomore be used for breeding
            // instead we just gonna use it as a cache for the prefab of current creature
            if (m_myPrefab == null)
            {
                m_myPrefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab());
                procreation.SetMyPrefab(m_myPrefab);
            }

            if (!procreation.TryGetComponent<OTAB_Creature>(out var creature))
            {
                Plugin.LogWarning("no creature component");
                return true;
            }

            // check if procreation is disabled while swimming
            if (creature.m_procreateWhileSwimming == false && m_character && m_character.IsSwimming())
            {
                return false;
            }

            // "__" => pseudo constants - do not change the values!
            var __myPosition = procreation.transform.position;
            var __myPartnerCheckRange = procreation.m_partnerCheckRange;
            var __myTotalCheckRange = procreation.m_totalCheckRange;
            var __myPrefabName = global::Utils.GetPrefabName(m_myPrefab.name);
            var __nowTicks = ZNet.instance.GetTime().Ticks;
            var __zNetScene = ZNetScene.instance;
            bool __isPregnant = procreation.IsPregnant();
            bool __isDue = procreation.IsDue();

            // s_ => valheim zdo vars
            int s_lovePoints = zdo.GetInt(ZDOVars.s_lovePoints);

            // z_ => zdo values, change via ZNetHelper
            string z_partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "");
            long z_partnerNotSeenSince = zdo.GetLong(Plugin.ZDOVars.z_partnerNotSeenSince, 0L);
            string z_offspringPrefab = zdo.GetString(Plugin.ZDOVars.z_offspringPrefab, "");
            int z_siblingsCounter = zdo.GetInt(Plugin.ZDOVars.z_siblingsCounter, 0);
            int z_offspringLevel = zdo.GetInt(Plugin.ZDOVars.z_offspringLevel, procreation.m_minOffspringLevel);
            int z_needPartner = zdo.GetInt(Plugin.ZDOVars.z_needPartner, 1);
            int z_offspringTamed = zdo.GetInt(Plugin.ZDOVars.z_offspringTamed, 1);

            

            bool doResetPartner = false;
            bool doResetOffspring = false;


            int RequiredPartners() => z_needPartner == 1 ? 1 : 0;

            int GetNearbyCountExcludeMyself(GameObject ofObj, float range)
            {
                var count = SpawnSystem.GetNrOfInstances(
                    ofObj, __myPosition, range,
                    eventCreaturesOnly: false,
                    procreationOnly: true);
                if (global::Utils.GetPrefabName(ofObj.name) == __myPrefabName)
                {
                    count -= 1;
                }
                return count;
            }

            //------------------------------------------------------
            //-- PARTNER
            //------------------------------------------------------

            // no partner saved - but something is stored in zdo
            if (procreation.m_seperatePartner == null && z_partnerPrefab.Length != 0)
            {
                var prefab = __zNetScene.GetPrefab(z_partnerPrefab);
                if (prefab)
                {
                    // use value in zdo as partner
                    procreation.m_seperatePartner = prefab;
                }
                else
                {
                    // invalid zdo -> clear
                    z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                }
            }

            // partner is selected and we need a partner
            if (procreation.m_seperatePartner != null && z_needPartner == 1)
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
                    int count = GetNearbyCountExcludeMyself(procreation.m_seperatePartner, __myTotalCheckRange);
                    keepPartner = count >= RequiredPartners();
                }

                if (keepPartner)
                {
                    // partner still nearby <3
                    z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                }
                else
                {
                    if (creature.m_partnerRecheckTicks > 0) // this feature can be turned off by setting it to 0 but would result in less pregnancies
                    {
                        if (z_partnerNotSeenSince == 0L)
                        {
                            // only set this value if it has not been set yet!
                            z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, __nowTicks, z_partnerNotSeenSince);
                        }

                        bool expired = z_partnerNotSeenSince != 0L && (__nowTicks - z_partnerNotSeenSince) > creature.m_partnerRecheckTicks;
                        if (expired) // creature is mourning to have lost the partner it just found =(
                        {
                            // this will trigger a new search for partner
                            z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                            procreation.m_seperatePartner = null;
                        }
                    }
                }
            }

            //if (m_seperatePartner == null && z_needPartner==1)
            if (procreation.m_seperatePartner == null) // always try to find new partner if its still empty
            {
                if (creature.HasPartnerList(out var partnerList))
                {
                    var foundPartner = RandomData.FindRandom<Data.Models.CreatureData.ProcreationPartnerData>(partnerList, out Data.Models.CreatureData.ProcreationPartnerData partnerEntry, entry =>
                    {
                        var prefab = __zNetScene.GetPrefab(entry.Prefab);
                        if (prefab == null) return 0; // zero weight => skip this one

                        return entry.Weight * GetNearbyCountExcludeMyself(prefab, __myPartnerCheckRange);
                    });

                    if (foundPartner)
                    {
                        procreation.m_seperatePartner = __zNetScene.GetPrefab(partnerEntry.Prefab);

                        z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, procreation.m_seperatePartner.name, z_partnerPrefab);
                        z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                        doResetOffspring = true;
                    }

                }
                else
                {
                    // no partner, it maybe can only breed with itself?
                }
            }

            // hint: at this point m_seperatePartner can still be null
            // next we gonna search for offspring where no partner might be needed afterall
            // after offspring part and before procreation part we gonna check for z_needPartner == 0 and set m_seperatePartner = m_myPrefab

            //------------------------------------------------------
            //-- OFFSPRING
            //------------------------------------------------------

            var m_offspringPrefab = procreation.GetOffspringPrefab();

            if (m_offspringPrefab == null && procreation.m_offspring != null && string.IsNullOrEmpty(z_offspringPrefab))
            {
                // handled by other mod or by default?
                string prefabName = global::Utils.GetPrefabName(procreation.m_offspring);
                m_offspringPrefab = __zNetScene.GetPrefab(prefabName);
                procreation.SetOffspringPrefab(m_offspringPrefab);
            }

            // try load offspring from zdo
            if (m_offspringPrefab == null)
            {
                if (z_offspringPrefab != "")
                {
                    var prefab = __zNetScene.GetPrefab(z_offspringPrefab);
                    if (prefab)
                    {
                        m_offspringPrefab = prefab;
                    }
                    else
                    {
                        z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                    }
                }
            }

            // still empty? search for new offspring
            if (m_offspringPrefab == null && creature.HasOffspringList(out var offspringList))
            {

                // search for random offspring
                var partnerName = procreation.m_seperatePartner?.name;
                var foundOffspring = RandomData.FindRandom<Data.Models.CreatureData.ProcreationOffspringData>(offspringList, out Data.Models.CreatureData.ProcreationOffspringData randomOffspring, entry =>
                {
                    var validPartner = entry.NeedPartner == false || (entry.NeedPartner && partnerName != null && (
                        (entry.NeedPartnerPrefab == null)
                        ||
                        (entry.NeedPartnerPrefab != null && partnerName == entry.NeedPartnerPrefab)
                    ));
                    return validPartner ? entry.Weight : 0;
                });
                
                if (foundOffspring)
                {
                    var offspring = __zNetScene.GetPrefab(randomOffspring.Prefab);

                    m_offspringPrefab = offspring;
                    z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, offspring.name, z_offspringPrefab);

                    if (randomOffspring.SpawnTamed)
                        z_offspringTamed = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringTamed, 1, z_offspringTamed);
                    else
                        z_offspringTamed = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringTamed, 0, z_offspringTamed);

                    if (randomOffspring.NeedPartner)
                        z_needPartner = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 1, z_needPartner);
                    else
                        z_needPartner = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 0, z_needPartner);

                    var levelOld = Mathf.Max(procreation.m_minOffspringLevel, m_character ? m_character.GetLevel() : procreation.m_minOffspringLevel);
                    var levelNew = levelOld;

                    // determ offspring level for levelup feature
                    // this is not the best place to set level of next offspring
                    // because the fate will be determined before creature gets pregnant
                    // but i want to store neither offspring-entry-index nor levelup-values into zdo
                    // so just roll the dice here...
                    if (randomOffspring.LevelUpChance != null && randomOffspring.MaxLevel != null)
                    {
                        var levelUpChance = (float)randomOffspring.LevelUpChance;
                        var levelUpMax = (int)randomOffspring.MaxLevel;
                        if (levelUpChance != 0)
                        {
                            var chance = UnityEngine.Random.value;
                            if (chance <= levelUpChance)
                            {
                                levelNew = levelNew + 1;
                                if (levelNew > levelUpMax)
                                {
                                    levelNew = levelUpMax;
                                }
                                Plugin.LogDebug($"Offspring '{offspring.name}' level up: {levelOld} -> {levelNew}");
                            }
                        }
                    }

                    z_offspringLevel = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, levelNew, z_offspringLevel);
                }
            }

            if (m_offspringPrefab == null)
            {
                // also abort getting siblings
                z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, 0, z_siblingsCounter);

                return false;
            }

            //------------------------------------------------------
            //-- REFRESH
            //------------------------------------------------------

            if (z_needPartner == 0 && procreation.m_seperatePartner != m_myPrefab)
            {
                // do not place this if block into offspring-section
                // because this place is the best one to keep the values up to date
                procreation.m_seperatePartner = m_myPrefab; // we are targeting ourself as partner for the next procreation 
                z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, procreation.m_seperatePartner.name, z_partnerPrefab);
            }

            if (procreation.m_seperatePartner == null) // this is not fatal! it just says that no partner is currently nearby
            {
                // just return false
                // the creature will surely find a lovly partner one day
                return false;
            }

            //------------------------------------------------------
            //-- PROCREATION
            //------------------------------------------------------

            // important: 
            // we always use m_seperatePartner as target partner
            // we never use m_myPrefab as target partner
            // for self-breeding m_seperatePartner has been set to m_myPrefab before

            if (__isPregnant)
            {
                if (__isDue)
                {

                    procreation.ResetPregnancy();

                    Vector3 forward = procreation.transform.forward;
                    Vector3 dir = forward;
                    if (procreation.m_spawnRandomDirection)
                    {
                        float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                        dir = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
                    }

                    float offset = (procreation.m_spawnOffsetMax > 0f)
                        ? UnityEngine.Random.Range(procreation.m_spawnOffset, procreation.m_spawnOffsetMax)
                        : procreation.m_spawnOffset;

                    GameObject spawned = UnityEngine.Object.Instantiate(
                        m_offspringPrefab,
                        __myPosition - dir * offset,
                        Quaternion.LookRotation(-forward, Vector3.up));

                    Character spawnedCharacter = spawned.GetComponent<Character>();
                    int level = Mathf.Max(procreation.m_minOffspringLevel, z_offspringLevel);
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
                    ThirdParty.Mods.CllCBridge.BequeathTraits(m_nview.GetComponent<Character>(), procreation.m_seperatePartner, spawned);

                    //---------------------------------
                    // sibling handling
                    //---------------------------------

                    {
                        bool unlimited = creature.m_maxSiblingsPerPregnancy < 0; // -1
                        bool canHaveMore = unlimited || z_siblingsCounter < creature.m_maxSiblingsPerPregnancy;
                        if (canHaveMore && UnityEngine.Random.value <= creature.m_extraSiblingChance)
                        {
                            ZNetUtils.SetLong(zdo, ZDOVars.s_pregnant, __nowTicks - TimeSpan.FromSeconds(procreation.m_pregnancyDuration).Ticks);
                            z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, z_siblingsCounter + 1);
                        }
                        else
                        {
                            doResetPartner = true;
                        }
                        doResetOffspring = true;
                    }

                }
            }
            else
            {
                var m_baseAI = procreation.GetBaseAI();

                bool canProcreate = UnityEngine.Random.value <= procreation.m_pregnancyChance && !(m_baseAI && m_baseAI.IsAlerted()) && !m_tameable.IsHungry();
                if (canProcreate)
                {
                    // max creatures check: only using m_seperatePartner + offspring (never m_myPrefab)
                    int totalAround = 0;
                    if (creature.HasMaxCreaturesPrefabs(out var prefabNames))
                    {
                        foreach (var prefabName in prefabNames)
                        {
                            totalAround += SpawnSystem.GetNrOfInstances(__zNetScene.GetPrefab(prefabName), __myPosition, __myTotalCheckRange);
                        }
                    }
                    else
                    {
                        // partners
                        totalAround += SpawnSystem.GetNrOfInstances(procreation.m_seperatePartner, __myPosition, __myTotalCheckRange);
                        // offsprings
                        totalAround += SpawnSystem.GetNrOfInstances(m_offspringPrefab, __myPosition, __myTotalCheckRange);
                    }

                    if (totalAround >= procreation.m_maxCreatures)
                    {
                        return false;
                    }

                    int partnersInRange = GetNearbyCountExcludeMyself(procreation.m_seperatePartner, __myPartnerCheckRange);
                    if (partnersInRange >= RequiredPartners())
                    {
                        if (partnersInRange > 0)
                        {
                            //m_loveEffects.Create(__myPosition, transform.rotation);
                            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_DisplayLoveEffect");
                        }

                        s_lovePoints++;
                        if (s_lovePoints >= procreation.m_requiredLovePoints)
                        {
                            procreation.MakePregnant();
                            s_lovePoints = 0;
                        }
                        s_lovePoints = ZNetUtils.SetInt(zdo, ZDOVars.s_lovePoints, s_lovePoints);
                    }
                }
            }

            if (doResetPartner)
            {
                z_partnerPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                z_partnerNotSeenSince = ZNetUtils.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                z_siblingsCounter = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_siblingsCounter, 0, z_siblingsCounter);
                procreation.m_seperatePartner = null;
                doResetOffspring = true;
            }

            if (doResetOffspring)
            {
                z_offspringPrefab = ZNetUtils.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                z_offspringLevel = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, procreation.m_minOffspringLevel, z_offspringLevel);
                z_needPartner = ZNetUtils.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 1, z_needPartner);
                m_offspringPrefab = null;
            }

            return false; // do not run original, we did the job
        }

        public static IReadOnlyList<string> GetProcreationHoverText(this Procreation procreation)
        {
            var m_nview = procreation.GetZNetView();
            var zdo = m_nview?.GetZDO();
            if (zdo == null)
                return Array.Empty<string>();

            var returnLines = new List<string>(capacity: 1);
            var L = Localization.instance;

            if (procreation.IsPregnant())
            {
                procreation.AddPregnancyLinesIfEnabled(returnLines, zdo, L);
                return returnLines; // pregnant -> no lovepoints line
            }

            procreation.AddLovePointsLinesIfEnabled(returnLines, L);
            return returnLines;
        }

        private static void AddPregnancyLinesIfEnabled(this Procreation procreation, List<string> returnLines, ZDO zdo, Localization L)
        {
            var trait = procreation.GetComponent<OTAB_ProcreationTrait>();

            var zTime = ZNet.instance.GetTime();
            long pregnantLong = zdo.GetLong(ZDOVars.s_pregnant, 0L);
            var dateTime = new DateTime(pregnantLong);
            var duration = trait.m_realPregnancyDuration;
            double secLeft = duration - (zTime - dateTime).TotalSeconds;

            returnLines.Add(Utils.StringUtils.FormatRelativeTime(
                secLeft,
                labelPositive:      L.Localize("$otab_hover_pregnancy_due"),
                labelPositiveAlt:   L.Localize("$otab_hover_pregnancy_due_alt"),
                labelNegative:      L.Localize("$otab_hover_pregnancy_overdue"),
                labelNegativeAlt:   L.Localize("$otab_hover_pregnancy_overdue_alt"),
                colorPositive:      Plugin.Configs.HoverColorGood.Value,
                colorNegative:      Plugin.Configs.HoverColorBad.Value
            ));
        }

        private static void AddLovePointsLinesIfEnabled(this Procreation procreation, List<string> returnLines, Localization L)
        {
            if (!Plugin.Configs.HoverShowLovePoints.Value)
                return;

            string partnerName = procreation.TryGetPartnerName();
            int lPoints = procreation.GetLovePoints();

            var color = lPoints > 0
                ? Plugin.Configs.HoverColorGood.Value
                : Plugin.Configs.HoverColorBad.Value;

            if (!string.IsNullOrEmpty(partnerName))
            {
                returnLines.Add(string.Format(
                    L.Localize("$otab_hover_love_points_with_partner"),
                    color,
                    lPoints,
                    procreation.m_requiredLovePoints,
                    L.Localize(partnerName)
                ));
            }
            else
            {
                returnLines.Add(string.Format(
                    L.Localize("$otab_hover_love_points"),
                    color,
                    lPoints,
                    procreation.m_requiredLovePoints
                ));
            }
        }

        private static string TryGetPartnerName(this Procreation procreation)
        {
            if (procreation.m_seperatePartner == null)
                return null;

            var partnerCharacter = procreation.m_seperatePartner.GetComponent<Character>();
            if (partnerCharacter == null)
                return null;

            return partnerCharacter.m_name;
        }

    }
}
