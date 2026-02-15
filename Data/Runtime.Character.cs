using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        internal static class Character
        {

            private static readonly Dictionary<int, string> groupWhenTamed = new Dictionary<int, string>();
            private static readonly HashSet<int> stickToFaction = new HashSet<int>();

            private static readonly Dictionary<int, IsEnemyCondition> canAttackTames = new Dictionary<int, IsEnemyCondition>();
            private static readonly Dictionary<int, IsEnemyCondition> canBeAttackedByTames = new Dictionary<int, IsEnemyCondition>();
            private static readonly Dictionary<int, IsEnemyCondition> canAttackPlayer = new Dictionary<int, IsEnemyCondition>();

            private static readonly Dictionary<int, float> animationScaling = new Dictionary<int, float>();

            public static void Reset()
            {
                groupWhenTamed.Clear();
                stickToFaction.Clear();

                canAttackTames.Clear();
                canBeAttackedByTames.Clear();
                canAttackPlayer.Clear();

                animationScaling.Clear();
            }

            public static void SetGroupWhenTamed(string name, string group)
            {
                groupWhenTamed[name.GetStableHashCode()] = group;
            }

            public static bool TryGetGroupWhenTamed(string name, out string group)
            {
                return groupWhenTamed.TryGetValue(name.GetStableHashCode(), out group);
            }

            public static void SetSticksToFaction(string name)
            {
                stickToFaction.Add(name.GetStableHashCode());
            }

            public static bool GetSticksToFaction(string name)
            {
                return stickToFaction.Contains(name.GetStableHashCode());
            }

            public static void SetCanAttackTames(string name, IsEnemyCondition cond)
            {
                if (cond == IsEnemyCondition.Never) return;
                canAttackTames[name.GetStableHashCode()] = cond;
            }

            public static IsEnemyCondition GetCanAttackTames(string name)
            {
                if (canAttackTames.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
                {
                    return c;
                }
                return IsEnemyCondition.Never;
            }

            public static void SetCanBeAttackedByTames(string name, IsEnemyCondition cond)
            {
                if (cond == IsEnemyCondition.Never) return;
                canBeAttackedByTames[name.GetStableHashCode()] = cond;
            }

            public static IsEnemyCondition GetCanBeAttackedByTames(string name)
            {
                if (canBeAttackedByTames.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
                {
                    return c;
                }
                return IsEnemyCondition.Never;
            }

            public static void SetCanAttackPlayer(string name, IsEnemyCondition cond)
            {
                if (cond == IsEnemyCondition.Never) return;
                canAttackPlayer[name.GetStableHashCode()] = cond;
            }

            public static IsEnemyCondition GetCanAttackPlayer(string name)
            {
                if (canAttackPlayer.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
                {
                    return c;
                }
                return IsEnemyCondition.Never;
            }

            public static void SetAnimationScaling(string name, float scale)
            {
                animationScaling[name.GetStableHashCode()] = scale;
            }

            public static bool GetAnimationScaling(string name, out float scale)
            {
                return animationScaling.TryGetValue(name.GetStableHashCode(), out scale);
            }

        }
    }
}
