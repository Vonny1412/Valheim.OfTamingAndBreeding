using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.Networking.UnityWebRequest;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ItemDrop), "RemoveOne")]
    static class ItemDrop_RemoveOne_Patch
    {
        static void Prefix(ItemDrop __instance)
        {
            // this is used to prevent creatures consume items not dropped by player

            Contexts.ConsumeItemContext.Clear();

            if (!Helpers.ZNetHelper.TryGetZDO(__instance, out ZDO zdo))
            {
                return;
            }

            Contexts.ConsumeItemContext.lastItemDroppedByAnyPlayer = zdo.GetInt(Plugin.ZDOVars.s_droppedByAnyPlayer, 0);
            Contexts.ConsumeItemContext.LastItemInstanceId = __instance.GetInstanceID();
            Contexts.ConsumeItemContext.HasValue = true;
        }
    }

    /** original method
    public bool RemoveOne()
    {
        if (!CanPickup())
        {
            RequestOwn();
            return false;
        }

        if (m_itemData.m_stack <= 1)
        {
            m_nview.Destroy();
            return true;
        }

        m_itemData.m_stack--;
        Save();
        return true;
    }
    **/

}
