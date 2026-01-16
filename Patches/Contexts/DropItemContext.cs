using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches.Contexts
{
    static class DropItemContext
    {
        //[ThreadStatic] public static Humanoid Dropper;
        [ThreadStatic] public static int DroppedByPlayer;
        public static void Clear()
        {
            DroppedByPlayer = 0;
        }
    }
}
