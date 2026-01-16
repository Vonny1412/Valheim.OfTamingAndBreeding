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
            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            var data = Data.Models.Egg.Get(prefabName);
            if (data != null)
            {
                // scale the egg once
                __instance.transform.localScale *= data.Item.scale;
            }
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
