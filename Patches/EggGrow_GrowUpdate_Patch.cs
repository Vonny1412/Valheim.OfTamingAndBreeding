using HarmonyLib;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Growup;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(EggGrow), "GrowUpdate")]
    static class EggGrow_GrowUpdate_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(EggGrow __instance)
        {
            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo, out ZNetView nview) || !nview.IsOwner())
            {
                return true; // let valheim handle
                             // will return anyway because of this:
                             //if (!m_nview.IsValid() || !m_nview.IsOwner() || m_item.m_itemData.m_stack > 1)
                             //{
                             //    UpdateEffects(num);
                             //   return;
                             //}
            }

            var eggGrowAPI = Internals.EggGrowAPI.GetOrCreate(__instance);
            return eggGrowAPI.GrowUpdate_Prefix(zdo);
        }

    }

    /** original method
    private void GrowUpdate()
    {
        float num = m_nview.GetZDO().GetFloat(ZDOVars.s_growStart);
        if (!m_nview.IsValid() || !m_nview.IsOwner() || m_item.m_itemData.m_stack > 1)
        {
            UpdateEffects(num);
            return;
        }

        if (CanGrow())
        {
            if (num == 0f)
            {
                num = (float)ZNet.instance.GetTimeSeconds();
            }
        }
        else
        {
            num = 0f;
        }

        m_nview.GetZDO().Set(ZDOVars.s_growStart, num);
        UpdateEffects(num);
        if (num > 0f && ZNet.instance.GetTimeSeconds() > (double)(num + m_growTime))
        {
            Character component = Object.Instantiate(m_grownPrefab, base.transform.position, base.transform.rotation).GetComponent<Character>();
            m_hatchEffect.Create(base.transform.position, base.transform.rotation);
            if ((bool)component)
            {
                component.SetTamed(m_tamed);
                component.SetLevel(m_item.m_itemData.m_quality);
            }

            m_nview.Destroy();
        }
    }
    **/

}
