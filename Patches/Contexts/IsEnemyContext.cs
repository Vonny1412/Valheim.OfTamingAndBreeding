using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches.Contexts
{
    // used in:
    // Character_IsTamed_Patch
    // BaseAI_IsEnemy_Patch
    internal static class IsEnemyContext
    {

        [System.ThreadStatic] public static int Depth;
        [System.ThreadStatic] public static Character TargetInstance;
        [System.ThreadStatic] public static bool Active;
    }
}
