using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Procreation), "Procreate")]
    static class Procreation_Procreate_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Procreation __instance)
        {
            try
            {
                var api = Internals.ProcreationAPI.GetOrCreate(__instance);
                return api.CustomProcreate(); // override vanilla
            }
            catch (Exception ex)
            {
                Plugin.LogFatal($"Procreation_Procreate_Patch.Prefix: {ex}");
                return true; // fail-open: allow vanilla + other mods
            }
        }
    }

    /** original method
    private void Procreate()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_tameable.IsTamed())
        {
            return;
        }

        if (m_offspringPrefab == null)
        {
            string prefabName = Utils.GetPrefabName(m_offspring);
            m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
            int prefab = m_nview.GetZDO().GetPrefab();
            m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
        }

        if (IsPregnant())
        {
            if (!IsDue())
            {
                return;
            }

            ResetPregnancy();
            GameObject original = m_offspringPrefab;
            if ((bool)m_noPartnerOffspring)
            {
                int nrOfInstances = SpawnSystem.GetNrOfInstances(m_seperatePartner ? m_seperatePartner : m_myPrefab, base.transform.position, m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true);
                if ((!m_seperatePartner && nrOfInstances < 2) || ((bool)m_seperatePartner && nrOfInstances < 1))
                {
                    original = m_noPartnerOffspring;
                }
            }

            Vector3 vector = base.transform.forward;
            if (m_spawnRandomDirection)
            {
                float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
            }

            float num = ((m_spawnOffsetMax > 0f) ? UnityEngine.Random.Range(m_spawnOffset, m_spawnOffsetMax) : m_spawnOffset);
            GameObject gameObject = UnityEngine.Object.Instantiate(original, base.transform.position - vector * num, Quaternion.LookRotation(-base.transform.forward, Vector3.up));
            Character component = gameObject.GetComponent<Character>();
            if ((bool)component)
            {
                component.SetTamed(m_tameable.IsTamed());
                component.SetLevel(Mathf.Max(m_minOffspringLevel, m_character ? m_character.GetLevel() : m_minOffspringLevel));
            }
            else
            {
                gameObject.GetComponent<ItemDrop>()?.SetQuality(Mathf.Max(m_minOffspringLevel, m_character ? m_character.GetLevel() : m_minOffspringLevel));
            }

            m_birthEffects.Create(gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            if (UnityEngine.Random.value <= m_pregnancyChance || ((bool)m_baseAI && m_baseAI.IsAlerted()) || m_tameable.IsHungry())
            {
                return;
            }

            int nrOfInstances2 = SpawnSystem.GetNrOfInstances(m_myPrefab, base.transform.position, m_totalCheckRange);
            int nrOfInstances3 = SpawnSystem.GetNrOfInstances(m_offspringPrefab, base.transform.position, m_totalCheckRange);
            if (nrOfInstances2 + nrOfInstances3 >= m_maxCreatures)
            {
                return;
            }

            int nrOfInstances4 = SpawnSystem.GetNrOfInstances(m_seperatePartner ? m_seperatePartner : m_myPrefab, base.transform.position, m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true);
            if ((bool)m_noPartnerOffspring || (((bool)m_seperatePartner || nrOfInstances4 >= 2) && (!m_seperatePartner || nrOfInstances4 >= 1)))
            {
                if (nrOfInstances4 > 0)
                {
                    m_loveEffects.Create(base.transform.position, base.transform.rotation);
                }

                int lovePoints = GetLovePoints();
                lovePoints++;
                m_nview.GetZDO().Set(ZDOVars.s_lovePoints, lovePoints);
                if (lovePoints >= m_requiredLovePoints)
                {
                    m_nview.GetZDO().Set(ZDOVars.s_lovePoints, 0);
                    MakePregnant();
                }
            }
        }
    }
    **/

}
