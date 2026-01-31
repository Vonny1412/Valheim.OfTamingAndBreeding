using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Helpers;

namespace OfTamingAndBreeding.Internals
{
    internal partial class ProcreationAPI : API.Procreation
    {

        private static readonly ConditionalWeakTable<Procreation, ProcreationAPI> instances
            = new ConditionalWeakTable<Procreation, ProcreationAPI>();
        public static ProcreationAPI GetOrCreate(Procreation __instance)
            => instances.GetValue(__instance, inst => new ProcreationAPI(inst));
        public static bool TryGet(Procreation __instance, out ProcreationAPI api)
            => instances.TryGetValue(__instance, out api);

        public readonly Values.CachedValue<float> RealPregnancyDuration;
        public readonly Values.CachedValue<int> OriginalMinOffspringLevel;

        public ProcreationAPI(Procreation __instance) : base(__instance)
        {
            RealPregnancyDuration = new Values.CachedValue<float>(
                () => m_pregnancyDuration,
                value => m_pregnancyDuration = value
            );
            OriginalMinOffspringLevel = new Values.CachedValue<int>(
                () => m_minOffspringLevel,
                value => m_minOffspringLevel = value
            );

            var data = Data.Models.Creature.Get(Utils.GetPrefabName(gameObject.name));
            var dataProcreation = data?.Procreation;
            if (dataProcreation != null)
            {
                _partnerRecheckTicks = TimeSpan.FromSeconds(dataProcreation.PartnerRecheckSeconds).Ticks;
            }

        }

        private readonly long? _partnerRecheckTicks;

        public bool Procreate_Prefix()
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_tameable.IsTamed())
            {
                return false;
            }
            if (!Helpers.ZNetHelper.TryGetZDO(m_nview, out ZDO zdo))
            {
                return false;
            }

            // original block, just keep it
            // m_myPrefab will nomore be used for breeding
            // instead we just gonna use it as a cache for the prefab of current creature
            if (m_myPrefab == null)
            {
                int prefab = zdo.GetPrefab();
                m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
            }

            var data = Data.Models.Creature.Get(Utils.GetPrefabName(gameObject.name));
            var dataProcreation = data?.Procreation;
            if (dataProcreation == null)
            {
                // we do not handle this creature! let valheim do the job
                return true;
            }

            // check if procreation is disabled while swimming
            if (dataProcreation.ProcreateWhileSwimming == false && m_character && m_character.IsSwimming())
            {
                return false;
            }

            // "__" => pseudo constants - do not change the values!
            var __myPosition = transform.position;
            var __myPartnerCheckRange = m_partnerCheckRange;
            var __myTotalCheckRange = m_totalCheckRange;
            var __myPrefabName = Utils.GetPrefabName(m_myPrefab.name);
            bool __isPregnant = IsPregnant();
            var __nowTicks = ZNet.instance.GetTime().Ticks;
            var __zNetScene = ZNetScene.instance;

            // s_ => valheim zdo vars
            int s_lovePoints = zdo.GetInt(ZDOVars.s_lovePoints);
            bool s_lovePointsZero = s_lovePoints == 0;

            // z_ => zdo values, change via ZNetHelper
            int z_doResetPartner = zdo.GetInt(Plugin.ZDOVars.z_doResetPartner, 0);
            string z_partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "");
            long z_partnerNotSeenSince = zdo.GetLong(Plugin.ZDOVars.z_partnerNotSeenSince, 0L);

            int z_doResetOffspring = zdo.GetInt(Plugin.ZDOVars.z_doResetOffspring, 0);
            string z_offspringPrefab = zdo.GetString(Plugin.ZDOVars.z_offspringPrefab, "");
            int z_offspringCounter = zdo.GetInt(Plugin.ZDOVars.z_offspringCounter, 0);
            int z_offspringLevel = zdo.GetInt(Plugin.ZDOVars.z_offspringLevel, 0);

            int z_needPartner = zdo.GetInt(Plugin.ZDOVars.z_needPartner, 1);

            int RequiredPartners() => z_needPartner == 1 ? 1 : 0;
            
            int GetNearbyCountExcludeMyself(GameObject ofObj)
            {
                var count = SpawnSystem.GetNrOfInstances(
                    ofObj, __myPosition, __myPartnerCheckRange,
                    eventCreaturesOnly: false,
                    procreationOnly: true);
                if (ofObj.name == __myPrefabName)
                {
                    count -= 1;
                }
                return count;
            }

            //------------------------------------------------------
            //-- PARTNER
            //------------------------------------------------------

            if (z_doResetPartner == 1)
            {
                z_doResetPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetPartner, 0, z_doResetPartner); // kinda useless but we gonna stick to the system
                z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                z_partnerNotSeenSince = ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);

                m_seperatePartner = null;

                z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 1, z_doResetOffspring); // new partner => new offspring
            }

            // no partner saved - but something is stored in zdo
            if (m_seperatePartner == null && z_partnerPrefab.Length != 0)
            {
                var prefab = __zNetScene.GetPrefab(z_partnerPrefab);
                if (prefab)
                {
                    // use value in zdo as partner
                    m_seperatePartner = prefab;
                }
                else
                {
                    // invalid zdo -> clear
                    z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                }
            }

            // partner is selected and we need a partner
            if (m_seperatePartner != null && z_needPartner == 1)
            {
                bool keepPartner;
                if (__isPregnant)
                {
                    // always keep while beeing pregnant
                    keepPartner = true;
                }
                else
                {
                    int count = GetNearbyCountExcludeMyself(m_seperatePartner);
                    keepPartner = count >= RequiredPartners();
                }

                if (keepPartner)
                {
                    // partner still nearby <3
                    z_partnerNotSeenSince = ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                }
                else
                {
                    if (dataProcreation.PartnerRecheckSeconds > 0) // this feature can be turned off by setting it to 0 but would result in less pregnancies
                    {
                        if (z_partnerNotSeenSince == 0L)
                        {
                            // only set this value if it has not been set yet!
                            z_partnerNotSeenSince = ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, __nowTicks, z_partnerNotSeenSince);
                        }

                        bool expired = z_partnerNotSeenSince != 0L && (__nowTicks - z_partnerNotSeenSince) > _partnerRecheckTicks;

                        //if (c_loveZero || expired)
                        if (expired) // creature is mourning to have lost the partner it just found =(
                        {

                            // this will trigger a new search for partner
                            z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);
                            m_seperatePartner = null;

                            // also reset love points, be ready for a fresh partner <3
                            s_lovePoints = ZNetHelper.SetInt(zdo, ZDOVars.s_lovePoints, 0, s_lovePoints);
                            s_lovePoints = GetLovePoints(); // should return same value but maybe someone patched this one
                            s_lovePointsZero = s_lovePoints == 0;
                        }
                    }
                }
            }

            //if (m_seperatePartner == null && z_needPartner==1)
            if (m_seperatePartner == null) // always try to find new partner if its still empty
            {
                if (dataProcreation.Partner != null && dataProcreation.Partner.Length > 0)
                {

                    var foundPartner = RandomData.FindRandom<Creature.ProcreationPartnerData>(dataProcreation.Partner, out Creature.ProcreationPartnerData partnerEntry, entry =>
                    {
                        var prefab = __zNetScene.GetPrefab(entry.Prefab);
                        if (prefab == null) return 0; // zero weight => skip this one

                        return entry.Weight * GetNearbyCountExcludeMyself(prefab);
                    });

                    if (foundPartner)
                    {
                        m_seperatePartner = __zNetScene.GetPrefab(partnerEntry.Prefab);

                        z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_seperatePartner.name, z_partnerPrefab);
                        z_partnerNotSeenSince = ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                        z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 1, z_doResetOffspring);
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

            if (z_doResetOffspring == 1)
            {
                z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 0, z_doResetOffspring);
                z_offspringPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                m_offspringPrefab = null;
            }

            if (m_offspringPrefab == null && m_offspring != null)
            {
                // handled by other mod or by default?
                string prefabName = Utils.GetPrefabName(m_offspring);
                m_offspringPrefab = __zNetScene.GetPrefab(prefabName);
            }

            // try load offspring from zdo
            if (m_offspringPrefab == null && !s_lovePointsZero)
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
                        z_offspringPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                    }
                }
            }

            // still empty? search for new offspring
            if (m_offspringPrefab == null)
            {

                // search for random offspring
                var partnerName = m_seperatePartner?.name;
                var foundOffspring = RandomData.FindRandom<Creature.ProcreationOffspringData>(dataProcreation.Offspring, out Creature.ProcreationOffspringData randomOffspring, entry =>
                {
                    var validPartner = entry.NeedPartner == false || (entry.NeedPartner && partnerName != null && (
                        (entry.NeedPartnerPrefab == null)
                        ||
                        (entry.NeedPartnerPrefab != null && partnerName == entry.NeedPartnerPrefab)
                    ));

                    return validPartner ? entry.Weight : 0;
                });

                GameObject offspring = null;
                if (foundOffspring)
                {
                    offspring = __zNetScene.GetPrefab(randomOffspring.Prefab);
                }

                if (!offspring)
                {   // something happend

                    // also abort getting siblings
                    z_offspringCounter = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_offspringCounter, 0, z_offspringCounter);

                    return false;
                }

                m_offspringPrefab = offspring;
                z_offspringPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, offspring.name, z_offspringPrefab);

                if (randomOffspring.NeedPartner)
                    z_needPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 1, z_needPartner);
                else
                    z_needPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_needPartner, 0, z_needPartner);

                // set m_minOffspringLevel for levelup feature
                // this is not the best place to set level of next offspring
                // because the fate will be determined before creature gets pregnant
                // but i want to store neither offspring-entry-index nor levelup-values into zdo
                // so just roll the dice here...
                var levelUpChance = randomOffspring.LevelUpChance;
                var levelUpMax = randomOffspring.MaxLevel;
                if (levelUpChance != 0)
                {
                    var chance = UnityEngine.Random.value;
                    if (chance <= levelUpChance)
                    {
                        var oldMinLevel = m_character
                            ? m_character.GetLevel()
                            : m_minOffspringLevel;
                        var newMinLevel = oldMinLevel + 1;
                        if (newMinLevel > levelUpMax)
                        {
                            newMinLevel = levelUpMax;
                        }
                        Plugin.LogDebug($"Offspring '{offspring.name}' level up: {oldMinLevel} -> {newMinLevel}");

                        z_offspringLevel = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, newMinLevel, z_offspringLevel);
                        //m_minOffspringLevel will be set later inside procreation section
                    }
                }
            }

            //------------------------------------------------------
            //-- REFRESH
            //------------------------------------------------------
            
            if (z_needPartner == 0)
            {
                // do not place this if block into offspring-section
                // because this place is the best one to keep the values up to date
                m_seperatePartner = m_myPrefab; // we are targeting ourself as partner for the next procreation 
                z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_seperatePartner.name, z_partnerPrefab);
                // remember: ZNetHelper.SetString will only increase zdo revision if value has been changed
                // so its okay to call this on every update
            }

            if (m_seperatePartner == null) // this is not fatal! it just says that no partner is currently nearby
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
                if (!IsDue())
                {
                    return false;
                }

                ResetPregnancy();

                //---------------------------------
                // sibling handling start
                //---------------------------------

                {

                    var doResetPartner = 1;

                    // check for siblings
                    z_offspringCounter += 1;
                    if (z_offspringCounter >= dataProcreation.MaxOffspringsPerPregnancy && dataProcreation.MaxOffspringsPerPregnancy != 0) // 0 = unlimited
                    {
                        // Max siblings reached -> stop chaining. Next pregnancy will pick partner/offspring fresh.
                        z_offspringCounter = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_offspringCounter, 0); // we just changed the value manually, omit last param
                    }
                    else
                    {
                        var chance = UnityEngine.Random.value;
                        if (chance <= dataProcreation.ExtraOffspringChance)
                        {
                            ZNetHelper.SetLong(zdo, ZDOVars.s_pregnant, __nowTicks - TimeSpan.FromSeconds(m_pregnancyDuration).Ticks);
                            z_offspringCounter = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_offspringCounter, z_offspringCounter);

                            doResetPartner = 0;
                        }
                    }

                    m_minOffspringLevel = z_offspringLevel;
                    z_offspringLevel = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_offspringLevel, 0, z_offspringLevel);
                    z_doResetPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetPartner, doResetPartner, z_doResetPartner);
                }

                //---------------------------------
                // sibling handling end
                //---------------------------------

                Vector3 forward = transform.forward;
                Vector3 dir = forward;
                if (m_spawnRandomDirection)
                {
                    float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                    dir = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
                }

                float offset = (m_spawnOffsetMax > 0f)
                    ? UnityEngine.Random.Range(m_spawnOffset, m_spawnOffsetMax)
                    : m_spawnOffset;

                GameObject spawned = UnityEngine.Object.Instantiate(
                    m_offspringPrefab,
                    __myPosition - dir * offset,
                    Quaternion.LookRotation(-forward, Vector3.up));

                Character ch = spawned.GetComponent<Character>();
                int level = Mathf.Max(m_minOffspringLevel, m_character ? m_character.GetLevel() : m_minOffspringLevel);

                if (ch != null)
                {
                    ch.SetTamed(m_tameable.IsTamed());
                    ch.SetLevel(level);
                }
                else
                {
                    spawned.GetComponent<ItemDrop>()?.SetQuality(level);
                }

                m_birthEffects.Create(spawned.transform.position, Quaternion.identity);

                // CLLC traits
                ThirdParty.Mods.CllCBridge.BequeathTraits(m_nview.GetComponent<Character>(), m_seperatePartner, spawned);
            }
            else
            {
                if (UnityEngine.Random.value <= m_pregnancyChance || ((bool)m_baseAI && m_baseAI.IsAlerted()) || m_tameable.IsHungry())
                {
                    return false;
                }

                // max creatures check: only using m_seperatePartner + offspring (never m_myPrefab)
                int totalAround = 0;
                if (data.Procreation.MaxCreaturesCountPrefabs != null)
                {
                    foreach (var prefabName in data.Procreation.MaxCreaturesCountPrefabs)
                    {
                        totalAround += SpawnSystem.GetNrOfInstances(__zNetScene.GetPrefab(prefabName), __myPosition, __myTotalCheckRange);
                    }
                }
                else
                {
                    // partners
                    totalAround += SpawnSystem.GetNrOfInstances(m_seperatePartner, __myPosition, __myTotalCheckRange);
                    // offsprings
                    totalAround += SpawnSystem.GetNrOfInstances(m_offspringPrefab, __myPosition, __myTotalCheckRange);
                }

                if (totalAround >= m_maxCreatures)
                {
                    return false;
                }

                int partnersInRange = GetNearbyCountExcludeMyself(m_seperatePartner);
                if (partnersInRange >= RequiredPartners())
                {
                    if (partnersInRange > 0)
                    {
                        m_loveEffects.Create(__myPosition, transform.rotation);
                    }

                    s_lovePoints++;
                    if (s_lovePoints >= m_requiredLovePoints)
                    {
                        MakePregnant();
                        s_lovePoints = 0;
                    }
                    s_lovePoints = ZNetHelper.SetInt(zdo, ZDOVars.s_lovePoints, s_lovePoints);
                }
            }

            return false; // do not run original, we did the job
        }

    }

}
