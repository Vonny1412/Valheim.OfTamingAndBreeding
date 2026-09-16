using System;

namespace OfTamingAndBreeding.Runtime
{
    internal static class ItemDropContext
    {
        //[ThreadStatic] public static Humanoid Dropper;
        [ThreadStatic] public static bool DroppedByPlayer;
        public static void Clear()
        {
            DroppedByPlayer = false;
        }
    }
}
