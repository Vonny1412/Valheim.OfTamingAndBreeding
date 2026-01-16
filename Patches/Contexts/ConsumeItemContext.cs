using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches.Contexts
{
    static class ConsumeItemContext
    {

        [ThreadStatic] public static int lastItemDroppedByAnyPlayer;
        [ThreadStatic] public static int LastItemInstanceId;
        [ThreadStatic] public static bool HasValue;

        public static void Clear()
        {
            lastItemDroppedByAnyPlayer = 0;
            LastItemInstanceId = 0;
            HasValue = false;
        }
    }
}
