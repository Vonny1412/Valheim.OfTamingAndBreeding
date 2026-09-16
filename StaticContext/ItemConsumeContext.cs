using System;

namespace OfTamingAndBreeding.StaticContext
{
    public static class ItemConsumeContext
    {
        [ThreadStatic] public static bool hasValue;
        [ThreadStatic] public static bool lastItemDroppedByPlayer;
        [ThreadStatic] public static int lastItemInstanceID;

        public static void Clear()
        {
            hasValue = false;
            lastItemDroppedByPlayer = false;
            lastItemInstanceID = 0;
        }
    }
}
