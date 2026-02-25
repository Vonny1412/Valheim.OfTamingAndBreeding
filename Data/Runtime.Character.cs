using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharacterFaction = global::Character.Faction;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        internal static class Character
        {

            private static readonly HashSet<int> stickToFaction = new HashSet<int>();
            private static readonly Dictionary<int, string> groupWhenTamed = new Dictionary<int, string>();
            private static readonly Dictionary<int, CharacterFaction> factionWhenTamed = new Dictionary<int, CharacterFaction>();

            private static readonly Dictionary<int, IsEnemyCondition> canAttackTamed = new Dictionary<int, IsEnemyCondition>();
            private static readonly Dictionary<int, IsEnemyCondition> canBeAttackedByTamed = new Dictionary<int, IsEnemyCondition>();
            private static readonly Dictionary<int, IsEnemyCondition> canAttackPlayer = new Dictionary<int, IsEnemyCondition>();

            public static void Reset()
            {
                stickToFaction.Clear();
                groupWhenTamed.Clear();
                factionWhenTamed.Clear();

                canAttackTamed.Clear();
                canBeAttackedByTamed.Clear();
                canAttackPlayer.Clear();

            }

            public static void SetSticksToFaction(string name)
            {
                stickToFaction.Add(name.GetStableHashCode());
            }

            public static bool GetSticksToFaction(string name)
            {
                return stickToFaction.Contains(name.GetStableHashCode());
            }

            public static void SetGroupWhenTamed(string name, string group)
            {
                groupWhenTamed[name.GetStableHashCode()] = group;
            }

            public static bool TryGetGroupWhenTamed(string name, out string group)
            {
                return groupWhenTamed.TryGetValue(name.GetStableHashCode(), out group);
            }

            public static void SetFactionWhenTamed(string name, CharacterFaction faction)
            {
                factionWhenTamed[name.GetStableHashCode()] = faction;
            }

            public static bool TryGetFactionWhenTamed(string name, out CharacterFaction faction)
            {
                return factionWhenTamed.TryGetValue(name.GetStableHashCode(), out faction);
            }

            public static void SetCanAttackTamed(string name, IsEnemyCondition cond)
            {
                if (cond == IsEnemyCondition.Never) return;
                canAttackTamed[name.GetStableHashCode()] = cond;
            }

            public static IsEnemyCondition GetCanAttackTamed(string name)
            {
                if (canAttackTamed.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
                {
                    return c;
                }
                return IsEnemyCondition.Never;
            }

            public static void SetCanBeAttackedByTamed(string name, IsEnemyCondition cond)
            {
                if (cond == IsEnemyCondition.Never) return;
                canBeAttackedByTamed[name.GetStableHashCode()] = cond;
            }

            public static IsEnemyCondition GetCanBeAttackedByTamed(string name)
            {
                if (canBeAttackedByTamed.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
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

        }
    }
}
