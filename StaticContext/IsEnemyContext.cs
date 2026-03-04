using System;

namespace OfTamingAndBreeding.StaticContext
{
    public static class IsEnemyContext
    {
        [ThreadStatic] public static int Depth;
        [ThreadStatic] public static Character TargetInstance;
        [ThreadStatic] public static bool Active;

        public static void Cleanup()
        {
            Depth--;
            if (Depth <= 0)
            {
                Depth = 0;
                Active = false;
                TargetInstance = null;
            }
        }
    }
}
