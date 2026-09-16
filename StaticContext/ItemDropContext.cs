using System;

namespace OfTamingAndBreeding.StaticContext
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
