using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class InventoryGridExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetInventory(this InventoryGrid inventoryGrid)
            => ValheimAPI.InventoryGrid.__IAPI_m_inventory_Invoker.Get(inventoryGrid);

    }
}
