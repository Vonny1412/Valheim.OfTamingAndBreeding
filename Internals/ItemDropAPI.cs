using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{
    /*
    internal class ItemDropAPI : InternalsAPI.Valheim.ItemDrop
    {
        private static readonly ConditionalWeakTable<ItemDrop, ItemDropAPI> instances
            = new ConditionalWeakTable<ItemDrop, ItemDropAPI>();
        public static ItemDropAPI GetOrCreate(ItemDrop __instance)
            => instances.GetValue(__instance, (ItemDrop inst) => new ItemDropAPI(inst));
        public static bool TryGetAPI(ItemDrop __instance, out ItemDropAPI api)
            => instances.TryGetValue(__instance, out api);

        public ItemDropAPI(ItemDrop __instance) : base(__instance)
        {
        }

        // class not used yet

    }
    */
}
