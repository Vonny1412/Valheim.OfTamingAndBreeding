using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(Tameable), "OnConsumedItem")]
    static class Tameable_OnConsumedItem_Patch
    {
        static bool Prefix(Tameable __instance, ItemDrop item)
        {
            if (Internals.TameableAPI.TryGet(__instance, out Internals.TameableAPI api))
            {
                return api.OnConsumedItem_Prefix(item);
            }
            return true;
        }
    }

    /** original method
    private void OnConsumedItem(ItemDrop item)
    {
        if (IsHungry())
        {
            m_sootheEffect.Create(m_character ? m_character.GetCenterPoint() : base.transform.position, Quaternion.identity);
        }

        ResetFeedingTimer();
    }
    **/

}
