using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Patches.Contexts
{
    internal static class DataContext
    {
        // note: this class is for faster data lookup
        // instead of `Data.DataBase<...>.Get(prefabName).componentName.fieldName`
        // the data can be accessed directly using these cached values
        // the values are added to the list when prefabs getting registered (check: Data/Handling/...Handler.cs)

        private static readonly HashSet<int> stickToFaction = new HashSet<int>();
        private static readonly HashSet<int> canAttackTames = new HashSet<int>();
        private static readonly HashSet<int> canBeAttackedByTames = new HashSet<int>();
        private static readonly Dictionary<int, float> animationScaling = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> fedDurations = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> eggScales = new Dictionary<int, float>();

        public static void Reset()
        {
            stickToFaction.Clear();
            canAttackTames.Clear();
            canBeAttackedByTames.Clear();
            animationScaling.Clear();
            fedDurations.Clear();
            eggScales.Clear();
        }

        public static void SetObjectSticksToFaction(string name) => stickToFaction.Add(name.GetStableHashCode());
        public static void SetObjectCanAttackTames(string name) => canAttackTames.Add(name.GetStableHashCode());
        public static void SetObjectCanBeAttackedByTames(string name) => canBeAttackedByTames.Add(name.GetStableHashCode());
        public static void SetObjectAnimationScaling(string name, float scale) => animationScaling[name.GetStableHashCode()] = scale;
        public static void SetObjectFedDuration(string name, float fedDuration) => fedDurations[name.GetStableHashCode()] = fedDuration;
        public static void SetEggScale(string name, float scale) => eggScales[name.GetStableHashCode()] = scale;

        public static bool ObjectSticksToFaction(string name) => stickToFaction.Contains(name.GetStableHashCode());
        public static bool ObjectCanAttackTames(string name) => canAttackTames.Contains(name.GetStableHashCode());
        public static bool ObjectCanBeAttackedByTames(string name) => canBeAttackedByTames.Contains(name.GetStableHashCode());
        public static bool GetObjectAnimationScaling(string name, out float scale) => animationScaling.TryGetValue(name.GetStableHashCode(), out scale);
        public static bool GetObjectFedDuration(string name, out float fedDuration) => fedDurations.TryGetValue(name.GetStableHashCode(), out fedDuration);
        public static float GetEggScale(string name) => eggScales.TryGetValue(name.GetStableHashCode(), out float scale) ? scale : 1;


        private static readonly Dictionary<int, ItemDrop> prefabItemDrops = new Dictionary<int, ItemDrop>();

        public static ItemDrop GetItemDropByPrefab(string prefabName)
        {
            var hash = prefabName.GetStableHashCode();
            if (prefabItemDrops.TryGetValue(hash, out ItemDrop itemDrop))
            {
                return itemDrop;
            }
            GameObject prefab = ObjectDB.instance.GetItemPrefab(prefabName);
            if (prefab == null)
            {
                prefabItemDrops.Add(hash, null);
                return null;
            }
            itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                prefabItemDrops.Add(hash, null);
                return null;
            }
            prefabItemDrops.Add(hash, itemDrop);
            return itemDrop;
        }





    }
}
