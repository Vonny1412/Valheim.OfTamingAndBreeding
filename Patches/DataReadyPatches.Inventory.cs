using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(Inventory), "AddItem", new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool Inventory_AddItem_Prefix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y, ref bool __result)
        {
            ItemDrop.ItemData itemAt = __instance.GetItemAt(x, y);
            if (itemAt == null) return true;
            if (item?.m_shared == null || itemAt?.m_shared == null) return true;
            if (item.m_shared.m_name != itemAt.m_shared.m_name) return true;
            if (StaticContext.EggDataContext.IsRegisteredEggSharedName(item.m_shared.m_name) == false)
            {
                return true;
            }

            // OTAB eggs store level in ItemData.quality.
            // Valheim ignores quality when MaxQuality == 1 -> mixed stacks can "promote".
            // Prevent stacking OTAB eggs with different quality.
            if (itemAt.m_quality != item.m_quality)
            {
                __result = false;
                return false;
            }
            return true;
        }

    }
}
