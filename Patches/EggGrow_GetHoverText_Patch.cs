using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(EggGrow), "GetHoverText")]
    static class EggGrow_GetHoverText_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static bool Prefix(EggGrow __instance, ref string __result)
        {
            var item = Internals.API.EggGrow.__IAPI_m_item_Invoker.Get(__instance);
            if (!item)
            {
                __result = "";
                return false;
            }
            // egg status is getting handled in ItemDrop_GetHoverText_Patch
            __result = item.GetHoverText();
            return false;
        }
    }

    /** original method
public string GetHoverText()
{
        if (!m_item)
        {
            return "";
        }

        if (!m_nview || !m_nview.IsValid())
        {
            return m_item.GetHoverText();
        }

        bool flag = m_nview.GetZDO().GetFloat(ZDOVars.s_growStart) > 0f;
        string text = ((m_item.m_itemData.m_stack > 1) ? "$item_chicken_egg_stacked" : (flag ? "$item_chicken_egg_warm" : "$item_chicken_egg_cold"));
        string hoverText = m_item.GetHoverText();
        int num = hoverText.IndexOf('\n');
        if (num > 0)
        {
            return hoverText.Substring(0, num) + " " + Localization.instance.Localize(text) + hoverText.Substring(num);
        }

        return m_item.GetHoverText();
}
    **/

}
