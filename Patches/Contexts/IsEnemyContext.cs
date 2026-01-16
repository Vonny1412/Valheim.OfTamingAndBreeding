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

        public static HashSet<int> prefabSticksToFaction = new HashSet<int>();
        public static HashSet<int> prefabAttacksTames = new HashSet<int>();
        static IsEnemyContext()
        {
            Data.DataLoader.OnDataReset(() => {
                prefabAttacksTames.Clear();
                prefabSticksToFaction.Clear();
            });
        }

        public static bool ObjectSticksToFaction(string name)
            => prefabSticksToFaction.Contains(name.GetStableHashCode());
        
        public static bool ObjectAttacksTames(string name)
            => prefabAttacksTames.Contains(name.GetStableHashCode());

        [System.ThreadStatic] public static int Depth;
        [System.ThreadStatic] public static Character TargetInstance;
        [System.ThreadStatic] public static bool Active;
    }
}
