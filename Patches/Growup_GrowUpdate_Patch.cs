using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(Growup), "GrowUpdate")]
    static class Growup_GrowUpdate_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(Growup __instance)
        {
            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo, out ZNetView nview) || !nview.IsOwner())
            {
                return true; // let valheim handle
            }

            var growupAPI = Internals.GrowupAPI.GetOrCreate(__instance);
            return growupAPI.GrowUpdate_Prefix(zdo);
        }
    }
    
    /** original method
    private void GrowUpdate()
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner() || !(m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)m_growTime))
        {
            return;
        }

        Character component = GetComponent<Character>();
        Character component2 = UnityEngine.Object.Instantiate(GetPrefab(), base.transform.position, base.transform.rotation).GetComponent<Character>();
        if ((bool)component && (bool)component2)
        {
            if (m_inheritTame)
            {
                component2.SetTamed(component.IsTamed());
            }

            component2.SetLevel(component.GetLevel());
        }

        m_nview.Destroy();
    }
    **/

}
