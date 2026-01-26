using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ItemDrop), "SetQuality")]
    static class ItemDrop_SetQuality_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(ItemDrop __instance)
        {
            // we need to multiply because localScale has already been set to variable scaling according to stuff like quality
            __instance.transform.localScale *= Contexts.DataContext.GetEggScale(Utils.GetPrefabName(__instance.gameObject.name));
        }
    }

    /** original method
    public void SetQuality(int quality)
    {
        m_itemData.m_quality = quality;
        base.transform.localScale = m_itemData.GetScale();
    }
    **/

}
