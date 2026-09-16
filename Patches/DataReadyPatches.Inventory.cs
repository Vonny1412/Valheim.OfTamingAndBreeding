using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {



        [HarmonyPatch(typeof(Inventory), "AddItem", new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static bool Inventory_AddItem_Prefix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y, ref bool __result)
        {
            ItemDrop.ItemData itemAt = __instance.GetItemAt(x, y);
            if (itemAt == null)
            {
                // slot is empty, continue
                return true;
            }
            if (item?.m_shared == null || itemAt?.m_shared == null)
            {
                // no shared data? dont handle it, continue
                return true;
            }
            if (item.m_shared.m_name != itemAt.m_shared.m_name)
            {
                // item in slot is different, continue
                return true;
            }
            if (StaticContext.ItemDataContext.IsRegisteredEggSharedName(item.m_shared.m_name) == false)
            {
                // is not a registered egg item, continue
                return true;
            }
            if (itemAt.m_quality == item.m_quality)
            {
                // same quality, continue
                return true;
            }

            // item on cursor and item in slot ...
            // ... got same name
            // ... and are both otab related eggs
            // ... and are of different quality

            // Valheim ignores quality when MaxQuality == 1 -> mixed stacks can "promote".
            // Prevent stacking OTAB eggs with different quality.
            __result = false;
            return false;
        }

    }
}
