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
        private static readonly HashSet<int> canBeAttackedByTames = new HashSet<int>();
        private static readonly Dictionary<int, float> animationScaling = new Dictionary<int, float>();

        public static void Reset()
        {
            stickToFaction.Clear();
            canAttackTames.Clear();
            canBeAttackedByTames.Clear();
            animationScaling.Clear();
        }

        public static void SetObjectSticksToFaction(string name) => stickToFaction.Add(name.GetStableHashCode());
        public static void SetObjectCanAttackTames(string name) => canAttackTames.Add(name.GetStableHashCode());
        public static void SetObjectCanBeAttackedByTames(string name) => canBeAttackedByTames.Add(name.GetStableHashCode());
        public static void SetObjectAnimationScaling(string name, float scale) => animationScaling[name.GetStableHashCode()] = scale;

        public static bool ObjectSticksToFaction(string name) => stickToFaction.Contains(name.GetStableHashCode());
        public static bool ObjectCanAttackTames(string name) => canAttackTames.Contains(name.GetStableHashCode());
        public static bool ObjectCanBeAttackedByTames(string name) => canBeAttackedByTames.Contains(name.GetStableHashCode());
        public static bool GetObjectAnimationScaling(string name, out float scale) => animationScaling.TryGetValue(name.GetStableHashCode(), out scale);






    }
}
