using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches.Contexts
{
    internal static class DataContext
    {
        private static readonly HashSet<int> stickToFaction = new HashSet<int>();
        private static readonly HashSet<int> canAttackTames = new HashSet<int>();

        static DataContext()
        {
            Data.DataLoader.OnDataReset(() => {
                stickToFaction.Clear();
                canAttackTames.Clear();
            });
        }

        public static bool ObjectSticksToFaction(string name)
            => stickToFaction.Contains(name.GetStableHashCode());

        public static bool ObjectCanAttackTames(string name)
            => canAttackTames.Contains(name.GetStableHashCode());

        public static void SetObjectSticksToFaction(string name)
            => stickToFaction.Add(name.GetStableHashCode());

        public static void SetObjectCanAttackTames(string name)
            => canAttackTames.Add(name.GetStableHashCode());






    }
}
