using System;

namespace OfTamingAndBreeding.StaticContext
{
    public static class ItemConsumeContext
    {
        [ThreadStatic] public static bool hasValue;
        [ThreadStatic] public static int lastItemDroppedByAnyPlayer;
        [ThreadStatic] public static int lastItemInstanceId;

        public static void Clear()
        {
            hasValue = false;
            lastItemDroppedByAnyPlayer = 0;
            lastItemInstanceId = 0;
        }
    }
}
