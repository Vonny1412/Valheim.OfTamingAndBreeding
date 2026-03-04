using System;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class ItemDropContext
    {
        //[ThreadStatic] public static Humanoid Dropper;
        [ThreadStatic] public static int DroppedByPlayer;
        public static void Clear()
        {
            DroppedByPlayer = 0;
        }
    }
}
