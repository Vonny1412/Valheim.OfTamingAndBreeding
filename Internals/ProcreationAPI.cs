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
    internal class ProcreationAPI : API.Procreation
    {

        private static readonly ConditionalWeakTable<Procreation, ProcreationAPI> instances
            = new ConditionalWeakTable<Procreation, ProcreationAPI>();
        public static ProcreationAPI GetOrCreate(Procreation __instance)
            => instances.GetValue(__instance, inst => new ProcreationAPI(inst));
        public static bool TryGetAPI(Procreation __instance, out ProcreationAPI api)
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

            // check for explicite max creatures
            if (s_lovePointsZero) // but only on zero love points, making it less restrictive
            {
                foreach (var kv in dataProcreation.MaxCreaturesExplicite)
                {
                    var maxCount = kv.Value;
                    var prefab = __zNetScene.GetPrefab(kv.Key); // valid prefabs already checked
                    int count = SpawnSystem.GetNrOfInstances(prefab, __myPosition, __myTotalCheckRange);
                    if (count >= maxCount)
                    {
                        return false;
                    }
                }
            }

            // z_ => zdo values, change via ZNetHelper
            int z_doResetPartner = zdo.GetInt(Plugin.ZDOVars.z_doResetPartner, 0);
            string z_partnerPrefab = zdo.GetString(Plugin.ZDOVars.z_partnerPrefab, "");
            long z_partnerNotSeenSince = zdo.GetLong(Plugin.ZDOVars.z_partnerNotSeenSince, 0L);

            int z_doResetOffspring = zdo.GetInt(Plugin.ZDOVars.z_doResetOffspring, 0);
            string z_offspringPrefab = zdo.GetString(Plugin.ZDOVars.z_offspringPrefab, "");
            int z_offspringCounter = zdo.GetInt(Plugin.ZDOVars.z_offspringCounter, 0);
            int z_offspringLevel = zdo.GetInt(Plugin.ZDOVars.z_offspringLevel, 0);

            int z_needPartner = zdo.GetInt(Plugin.ZDOVars.z_needPartner, 0);

            int RequiredPartners() // warning: always make sure m_seperatePartner!=null - this helper does not check!
            {
                // If partner is own prefab, count includes self -> need 2 for a "real partner".
                // If partner is different prefab, need 1 instance of that prefab.
                return __myPrefabName == m_seperatePartner.name ? 2 : 1;
            }

            int GetNrOfPartnersAround(GameObject ofObj)
            {
                return SpawnSystem.GetNrOfInstances(
                    ofObj, __myPosition, __myPartnerCheckRange,
                    eventCreaturesOnly: false,
                    procreationOnly: true);
            }

            //------------------------------------------------------
            //-- PARTNER
            //------------------------------------------------------

            // IMPORTANT:
            // Partner lists are explicit.
            // If an offspring does NOT require a partner, the creature's own prefab
            // MUST still be listed here as a valid partner.
            //
            // Reason:
            // This allows defining creatures that can breed only with OTHER species,
            // but never with their own kind.
            //
            // The Partner list is NOT "implicit + self".
            // Only prefabs listed here are considered valid partners.

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

            // partner is selected
            if (m_seperatePartner != null)
            {
                bool keepPartner;
                if (__isPregnant)
                {
                    // always keep while beeing pregnant
                    keepPartner = true;
                }
                else
                {
                    int count = GetNrOfPartnersAround(m_seperatePartner);
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

                        var expireTicks = _partnerRecheckTicks;
                        bool expired = z_partnerNotSeenSince != 0L && (__nowTicks - z_partnerNotSeenSince) > expireTicks;

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

            if (m_seperatePartner == null) //  && loveZero
            {

                var foundPartner = RandomData.FindRandom<Creature.ProcreationPartnerData>(dataProcreation.Partner, out Creature.ProcreationPartnerData partnerEntry, entry =>
                {
                    var prefab = __zNetScene.GetPrefab(entry.Prefab);
                    if (prefab == null) return 0; // zero weight => skipp this one

                    int count = GetNrOfPartnersAround(prefab);
                    if (prefab.name == __myPrefabName)
                    {   // thats right! do not include ourself!
                        count -= 1;
                    }

                    return entry.Weight * count;
                });
                if (!foundPartner)
                {
                    // this could happen. no needed partners around? maybe next try
                    return false;
                }

                m_seperatePartner = __zNetScene.GetPrefab(partnerEntry.Prefab);
                // we do not check for != null, already done here just before

                z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, m_seperatePartner.name, z_partnerPrefab);
                z_partnerNotSeenSince = ZNetHelper.SetLong(zdo, Plugin.ZDOVars.z_partnerNotSeenSince, 0L, z_partnerNotSeenSince);
                z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 1, z_doResetOffspring);
            }
            
            if (m_seperatePartner == null && !__isPregnant)
            {
                // we are in an invalid state -> rebuild partner & offspring next tick
                z_doResetPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetPartner, 1, z_doResetPartner);
                z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 1, z_doResetOffspring);
                m_offspringPrefab = null;
                m_noPartnerOffspring = null;

                if (z_partnerPrefab.Length != 0)
                    z_partnerPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_partnerPrefab, "", z_partnerPrefab);

                if (z_offspringPrefab.Length != 0)
                    z_offspringPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);

                // remember: we do keep m_seperatePartner if we are pregnant
                // having m_seperatePartner==null means no partner and no pregnancy
                // m_noPartnerOffspring needs atleast m_seperatePartner set to own prefab!
                // so we can abort at this point

                return false; 
            }

            //------------------------------------------------------
            //-- OFFSPRING
            //------------------------------------------------------

            if (z_doResetOffspring == 1)
            {
                z_doResetOffspring = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetOffspring, 0, z_doResetOffspring);
                z_offspringPrefab = ZNetHelper.SetString(zdo, Plugin.ZDOVars.z_offspringPrefab, "", z_offspringPrefab);
                m_offspringPrefab = null;
                m_noPartnerOffspring = null;
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

                // wait! we need m_seperatePartner to be set. even when using no-partner procreation
                // m_offspringPrefab is currently null so its okay to just abort and force searching for a new partner
                // why? because we are using a strict system:
                // - partnerlist cannot be empty
                // - when using no-partner procreation NeedPartner need to be set to false and NeedPartnerPrefab need to point to our own prefab
                // - because in no-partner procreation the creature acts as its own partner
                if (m_seperatePartner == null)
                {
                    z_doResetPartner = ZNetHelper.SetInt(zdo, Plugin.ZDOVars.z_doResetPartner, 1, z_doResetPartner); // will also trigger search for new offspring
                    return false;
                }

                // search for random offspring
                var partnerName = m_seperatePartner.name;
                var foundOffspring = RandomData.FindRandom<Creature.ProcreationOffspringData>(dataProcreation.Offspring, out Creature.ProcreationOffspringData randomOffspring, entry =>
                {
                    var validPartner = entry.NeedPartner == false || entry.NeedPartnerPrefab == null || (partnerName == entry.NeedPartnerPrefab);
                    if (validPartner)
                    {
                        return entry.Weight;
                    }
                    return 0;
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
                // so just roll the dice...
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

            switch (z_needPartner)
            {
                case 0: // we do NOT need a partner
                    m_noPartnerOffspring = m_offspringPrefab;
                    break;
                case 1: // we DO need a partner
                    m_noPartnerOffspring = null;
                    break;
            }

            if (m_seperatePartner == null)
            {
                // try to load partner prefab from zdo
                m_seperatePartner = __zNetScene.GetPrefab(z_partnerPrefab);
            }

            if (m_seperatePartner == null || m_offspringPrefab == null)
            {
                // do not continue with invalid prefabs
                // this should never happen but whatever
                return false;
            }

            //------------------------------------------------------
            //-- PROCREATION
            //------------------------------------------------------

            // important: 
            // we always use m_seperatePartner as target partner
            // we never use m_myPrefab as target partner

            // Reminder:
            // Partner handling is explicit.
            // m_seperatePartner may intentionally point to the creature's own prefab.
            // This is required to support "no self-breeding" configurations.

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

                GameObject original = m_offspringPrefab;

                // vanilla behavior: only check partner presence if m_noPartnerOffspring exists
                if (m_noPartnerOffspring != null)
                {
                    int partnersInRange = GetNrOfPartnersAround(m_seperatePartner);
                    // if partner missing -> fallback offspring
                    if (partnersInRange < RequiredPartners())
                    {
                        original = m_noPartnerOffspring; // we can do this because we checked if (m_noPartnerOffspring != null)
                    }
                }

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
                    original,
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
            }
            else
            {
                if (UnityEngine.Random.value <= m_pregnancyChance || ((bool)m_baseAI && m_baseAI.IsAlerted()) || m_tameable.IsHungry())
                {
                    return false;
                }

                // max creatures check: only using m_seperatePartner + offspring (never m_myPrefab)
                int partnersTotal = SpawnSystem.GetNrOfInstances(m_seperatePartner, __myPosition, __myTotalCheckRange);
                int offspringTotal = SpawnSystem.GetNrOfInstances(m_offspringPrefab, __myPosition, __myTotalCheckRange);
                if (partnersTotal + offspringTotal >= m_maxCreatures)
                {
                    return false;
                }

                int partnersInRange = GetNrOfPartnersAround(m_seperatePartner);
                bool hasRequiredPartner = partnersInRange >= RequiredPartners();

                // vanilla-equivalent gate:
                // - if noPartnerOffspring exists -> allow lovePoints even without partner
                // - else -> require partner presence
                if (z_needPartner == 0 || hasRequiredPartner)
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



        public string GetProcreationHoverText()
        {
            ZDO zDO = m_nview.GetZDO();
            if (zDO == null)
            {
                return "";
            }

            var text = new List<string>();

            var L = Localization.instance;

            var partner = m_seperatePartner != null ? m_seperatePartner : m_myPrefab;
            /*
            // debugging
            var partner = m_seperatePartner != null ? m_seperatePartner.name : (m_myPrefab != null ? m_myPrefab.name : "-");
            var offspring = m_offspringPrefab != null ? m_offspringPrefab.name : "-";
            text.Add($"Partner: {partner}");
            text.Add($"Offspring: {offspring}");
            */

            if (IsPregnant())
            {
                if (Plugin.Configs.HoverShowPregnancy.Value)
                {
                    //text.Add(String.Format(L.Localize("$tmt_hover_pregnant"), "green"));

                    var duration = RealPregnancyDuration.GetValue();

                    if (Plugin.Configs.HoverShowPregnancyTimer.Value)
                    {
                        long @pregnantLong = zDO.GetLong(ZDOVars.s_pregnant, 0L);
                        DateTime dateTime = new DateTime(@pregnantLong);
                        double secLeft = duration - (ZNet.instance.GetTime() - dateTime).TotalSeconds;

                        text.Add(Helpers.StringHelper.FormatRelativeTime(
                            secLeft,
                            labelPositive: L.Localize("$tmt_hover_pregnancy_due"),
                            labelNegative: L.Localize("$tmt_hover_pregnancy_overdue"),
                            labelAltPositive: L.Localize("$tmt_hover_pregnancy_due_alt"),
                            labelAltNegative: L.Localize("$tmt_hover_pregnancy_overdue_alt"),
                            colorPositive: Plugin.Configs.HoverColorGood.Value,
                            colorNegative: Plugin.Configs.HoverColorBad.Value
                        ));

                        /*
                        if (Plugin.Configs.HoverShowDurations.Value)
                        {
                            text.Add(string.Format(L.Localize("$tmt_hover_duration"), Plugin.Configs.HoverColorNormal.Value, duration));
                        }
                        */
                    }
                }
            }
            else
            {
                if (Plugin.Configs.HoverShowLovePoints.Value)
                {
                    // 
                    string partnerName = null;
                    if (partner != null)
                    {
                        var partnerCharacter = partner.GetComponent<Character>();
                        if (partnerCharacter != null)
                        {
                            partnerName = partnerCharacter.m_name;
                        }
                    }

                    var lPoints = GetLovePoints();
                    var color = lPoints > 0 ? Plugin.Configs.HoverColorGood.Value : Plugin.Configs.HoverColorBad.Value;
                    if (partnerName != null)
                    {
                        text.Add(String.Format(L.Localize("$tmt_hover_lovepoints_withPartner"), color, lPoints, m_requiredLovePoints, L.Localize(partnerName)));
                    }
                    else
                    {
                        text.Add(String.Format(L.Localize("$tmt_hover_lovepoints"), color, lPoints, m_requiredLovePoints));
                    }
                }
            }

            return string.Join("\n", text.Where((string line) => line.Trim() != ""));
        }

    }

}
