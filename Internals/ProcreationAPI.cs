using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using YamlDotNet.Core;

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
        }

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

            /*
            if (m_myPrefab == null)
            {
                int prefab = zdo.GetPrefab();
                m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
            }
            // do not uncomment me, i dont know why but its okay
            */

            var data = Data.Models.Creature.Get(Utils.GetPrefabName(name));
            var dataProcreation = data?.Procreation;
            if (dataProcreation == null)
            {
                return true;
            }

            // check if procreation is disabled while swimming
            if (dataProcreation.ProcreateWhileSwimming == false && m_character.IsSwimming())
            {
                return false;
            }

            int lovePoints = GetLovePoints();
            bool loveZero = lovePoints == 0;
            bool isPregnant = IsPregnant();

            //------------------------------------------------------
            //-- PARTNER
            //------------------------------------------------------

            if (zdo.GetInt(Plugin.ZDOVars.s_doResetPartner, 0) == 1)
            {
                zdo.Set(Plugin.ZDOVars.s_doResetPartner, 0);
                zdo.Set(Plugin.ZDOVars.s_partnerPrefab, "");
                zdo.Set(Plugin.ZDOVars.s_partnerLastSeen, 0L);
                m_myPrefab = null;
            }

            // check for custom partners
            if (dataProcreation.Partner != null && dataProcreation.Partner.Length > 0)
            {
                var nowTicks = ZNet.instance.GetTime().Ticks;
                var savedName = zdo.GetString(Plugin.ZDOVars.s_partnerPrefab, "");
                var lastSeen = zdo.GetLong(Plugin.ZDOVars.s_partnerLastSeen, 0L);

                // no partner saved
                if (!m_myPrefab && !string.IsNullOrEmpty(savedName))
                {
                    var prefab = ZNetScene.instance.GetPrefab(savedName);
                    if (prefab)
                    {
                        m_myPrefab = prefab;
                    }
                    else
                    {
                        // invalid prefab -> clear
                        zdo.Set(Plugin.ZDOVars.s_partnerPrefab, "");
                        m_myPrefab = null;
                    }
                }

                if (m_myPrefab)
                {
                    bool keepPartner;
                    if (isPregnant)
                    {
                        keepPartner = true;
                    }
                    else
                    {
                        int count = SpawnSystem.GetNrOfInstances(
                            m_myPrefab, transform.position, m_partnerCheckRange,
                            eventCreaturesOnly: false,
                            procreationOnly: true);

                        keepPartner = count > 0;
                    }
                    if (keepPartner)
                    {
                        // partner still available
                        zdo.Set(Plugin.ZDOVars.s_partnerLastSeen, nowTicks);
                    }
                    else
                    {
                        if (dataProcreation.PartnerRecheckSeconds > 0) // this feature can be turned off by setting it to 0
                        {
                            var expireTicks = TimeSpan.FromSeconds(dataProcreation.PartnerRecheckSeconds).Ticks;
                            bool expired = lastSeen != 0 && (nowTicks - lastSeen) > expireTicks;

                            if (loveZero || expired)
                            {
                                // this will trigger a new search for partner
                                m_myPrefab = null;
                                zdo.Set(Plugin.ZDOVars.s_partnerPrefab, "");

                                // also reset love points, be ready for a fresh partner <3
                                m_nview.GetZDO().Set(ZDOVars.s_lovePoints, 0);
                                lovePoints = GetLovePoints(); // maybe someone patched this one
                                loveZero = lovePoints == 0;
                            }
                        }
                    }
                }

                if (!m_myPrefab) //  && loveZero
                {

                    var searchPosition = transform.position;
                    var searchRange = m_partnerCheckRange;
                    var foundRandomPartner = RandomData.FindRandom<Creature.ProcreationPartnerData>(dataProcreation.Partner, out Creature.ProcreationPartnerData partnerEntry, entry =>
                    {
                        var prefab = ZNetScene.instance.GetPrefab(entry.Prefab);
                        if (prefab == null) return 0;
                        int count = SpawnSystem.GetNrOfInstances(
                            prefab, searchPosition, searchRange,
                            eventCreaturesOnly: false,
                            procreationOnly: true);
                        return entry.Weight * count;
                    });
                    if (!foundRandomPartner)
                    {
                        return false;
                    }

                    m_myPrefab = ZNetScene.instance.GetPrefab(partnerEntry.Prefab);

                    zdo.Set(Plugin.ZDOVars.s_partnerPrefab, m_myPrefab.name);
                    zdo.Set(Plugin.ZDOVars.s_partnerLastSeen, nowTicks);

                    zdo.Set(Plugin.ZDOVars.s_doResetOffspring, 1);
                }
            }
            else
            {
                //return false;
                // i dont care...
            }

            //------------------------------------------------------
            //-- OFFSPRING
            //------------------------------------------------------

            if (zdo.GetInt(Plugin.ZDOVars.s_doResetOffspring, 0) == 1)
            {
                zdo.Set(Plugin.ZDOVars.s_doResetOffspring, 0);
                zdo.Set(Plugin.ZDOVars.s_offspringPrefab, "");
                m_offspringPrefab = null;
                m_noPartnerOffspring = null;
            }

            if (m_offspringPrefab == null && m_offspring != null)
            {
                // handled by other mod or by default?
                string prefabName = Utils.GetPrefabName(m_offspring);
                m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
            }

            // try load offspring from zdo
            if (m_offspringPrefab == null && !loveZero)
            {
                var savedOffspring = zdo.GetString(Plugin.ZDOVars.s_offspringPrefab, "");
                if (!string.IsNullOrEmpty(savedOffspring))
                {
                    var prefab = ZNetScene.instance.GetPrefab(savedOffspring);
                    if (prefab)
                    {
                        m_offspringPrefab = prefab;
                    }
                    else
                    {
                        zdo.Set(Plugin.ZDOVars.s_offspringPrefab, "");
                        zdo.Set(Plugin.ZDOVars.s_maxCreatures, 0);
                        zdo.Set(Plugin.ZDOVars.s_needPartner, 1);
                    }
                }
            }
            // search for new offspring
            if (m_offspringPrefab == null)
            {
                // search for random offspring

                var searchWithNeededPartnerName = m_myPrefab?.name;
                var foundRandomOffspring = RandomData.FindRandom<Creature.ProcreationOffspringData>(dataProcreation.Offspring, out Creature.ProcreationOffspringData randomOffspring, entry =>
                {
                    //var consumedCorrect = entry.needFoodPrefab == null || (entry.needFoodPrefab == consumedItem);
                    var validPartner = entry.NeedPartner == false || entry.NeedPartnerPrefab == null || (searchWithNeededPartnerName != null && searchWithNeededPartnerName == entry.NeedPartnerPrefab);
                    if (validPartner /* && consumedCorrect */ )
                    {
                        return entry.Weight;
                    }
                    return 0;
                });
                if (!foundRandomOffspring)
                {
                    return false;
                }

                var offspring = ZNetScene.instance.GetPrefab(randomOffspring.Prefab);
                if (!offspring)
                {
                    // something happend
                    return false;
                }

                m_offspringPrefab = offspring;

                zdo.Set(Plugin.ZDOVars.s_offspringPrefab, offspring.name);
                zdo.Set(Plugin.ZDOVars.s_maxCreatures, randomOffspring.MaxCreatures);
                if (randomOffspring.NeedPartner)
                {
                    zdo.Set(Plugin.ZDOVars.s_needPartner, 1);
                }
                else
                {
                    zdo.Set(Plugin.ZDOVars.s_needPartner, 0);
                }

                // set m_minOffspringLevel for levelup feature
                // this is not the best place to set level of next offspring
                // because the fate will be determined before creature gets pregnant
                // but i want to store neither offspring-entry-index nor levelup-values into zdo
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
                        zdo.Set(Plugin.ZDOVars.s_offspringLevel, newMinLevel);
                        //m_minOffspringLevel will be set inside PrepareBreeding()
                    }
                }
            }

            // at the end refresh some values
            m_maxCreatures = zdo.GetInt(Plugin.ZDOVars.s_maxCreatures, 0);
            switch(zdo.GetInt(Plugin.ZDOVars.s_needPartner, 1))
            {
                case 0:
                    m_noPartnerOffspring = m_offspringPrefab;
                    break;
                case 1:
                    m_noPartnerOffspring = null;
                    break;
            }
            if (m_myPrefab == null)
            {
                // try to load partner prefab from zdo
                m_myPrefab = ZNetScene.instance.GetPrefab(zdo.GetString(Plugin.ZDOVars.s_partnerPrefab, ""));
            }

            if (m_myPrefab == null || m_offspringPrefab == null)
            {
                // do not let Procreate() run with invalid prefabs
                // todo: show debug infos in log
                return false;
            }
            return true;
        }

        public void PrepareBreeding()
        {
            /* call stack:
Procreation.Procreate() {
if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_tameable.IsTamed())
{
    return;
}
...
if (IsPregnant())
{
    if (!IsDue())
    {
        return;
    }
    ResetPregnancy(); <- PrepareBreeding() gets called after this
...

            BUT: we cannot guarantee that it has realy been called by Procreate()
            */
            if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_tameable.IsTamed())
            {
                return;
            }
            if (!Helpers.ZNetHelper.TryGetZDO(m_nview, out ZDO zdo))
            {
                return;
            }

            var data = Data.Models.Creature.Get(m_myPrefab.name);
            var dataProcreation = data?.Procreation;
            if (dataProcreation == null)
            {
                return;
            }

            var doResetPartner = 1;
            var doResetOffspring = 1;

            // check for siblings
            int siblings = zdo.GetInt(Plugin.ZDOVars.s_offspringCounter, 0);
            siblings += 1;
            if (siblings > dataProcreation.MaxOffspringsPerPregnancy)
            {
                zdo.Set(Plugin.ZDOVars.s_offspringCounter, 0);
            }
            else
            {
                var chance = UnityEngine.Random.value;
                if (chance <= dataProcreation.ExtraOffspringChance)
                {
                    long ticks = ZNet.instance.GetTime().Ticks;
                    ticks -= TimeSpan.FromSeconds(m_pregnancyDuration).Ticks;
                    m_nview.GetZDO().Set(ZDOVars.s_pregnant, ticks);

                    zdo.Set(Plugin.ZDOVars.s_offspringCounter, siblings);
                    doResetPartner = 0;
                }
            }
            
            m_minOffspringLevel = zdo.GetInt(Plugin.ZDOVars.s_offspringLevel, 0);
            zdo.Set(Plugin.ZDOVars.s_offspringLevel, 0);

            zdo.Set(Plugin.ZDOVars.s_doResetPartner, doResetPartner);
            zdo.Set(Plugin.ZDOVars.s_doResetOffspring, doResetOffspring);
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
