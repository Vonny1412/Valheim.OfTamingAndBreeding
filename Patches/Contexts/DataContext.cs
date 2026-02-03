using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using OfTamingAndBreeding.Data.Models.SubData;
namespace OfTamingAndBreeding.Patches.Contexts
{
    internal static class DataContext
    {
        // note: this class is for faster data lookup
        // instead of `Data.DataBase<...>.Get(prefabName).componentName.fieldName`
        // the data can be accessed directly using these cached values
        // the values are added to the list when prefabs getting registered (check: Data/Handling/...Handler.cs)











        private static readonly Dictionary<int, IsEnemyCondition> canAttackTames = new Dictionary<int, IsEnemyCondition>();
        private static readonly Dictionary<int, IsEnemyCondition> canBeAttackedByTames = new Dictionary<int, IsEnemyCondition>();
        private static readonly Dictionary<int, IsEnemyCondition> canAttackPlayer = new Dictionary<int, IsEnemyCondition>();
        public static void SetCanAttackTames(string name, IsEnemyCondition cond)
        {
            if (cond == IsEnemyCondition.Never) return;
            canAttackTames[name.GetStableHashCode()] = cond;
        }
        public static void SetCanBeAttackedByTames(string name, IsEnemyCondition cond)
        {
            if (cond == IsEnemyCondition.Never) return;
            canBeAttackedByTames[name.GetStableHashCode()] = cond;
        }
        public static void SetCanAttackPlayer(string name, IsEnemyCondition cond)
        {
            if (cond == IsEnemyCondition.Never) return;
            canAttackPlayer[name.GetStableHashCode()] = cond;
        }
        public static IsEnemyCondition GetCanAttackTames(string name)
        {
            if (canAttackTames.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
            {
                return c; 
            }
            return IsEnemyCondition.Never;
        }
        public static IsEnemyCondition GetCanBeAttackedByTames(string name)
        {
            if (canBeAttackedByTames.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
            {
                return c;
            }
            return IsEnemyCondition.Never;
        }
        public static IsEnemyCondition GetCanAttackPlayer(string name)
        {
            if (canAttackPlayer.TryGetValue(name.GetStableHashCode(), out IsEnemyCondition c))
            {
                return c;
            }
            return IsEnemyCondition.Never;
        }




        private static readonly HashSet<int> stickToFaction = new HashSet<int>();
        public static void SetSticksToFaction(string name) => stickToFaction.Add(name.GetStableHashCode());
        public static bool GetSticksToFaction(string name) => stickToFaction.Contains(name.GetStableHashCode());



        private static readonly Dictionary<int, string> groupWhenTamed = new Dictionary<int, string>();
        public static void SetGroupWhenTamed(string name, string group) => groupWhenTamed[name.GetStableHashCode()] = group;
        public static bool GetGroupWhenTamed(string name, out string group) => groupWhenTamed.TryGetValue(name.GetStableHashCode(), out group);




        private static readonly Dictionary<int, float> fedDurations = new Dictionary<int, float>();
        public static void SetFedDuration(string name, float fedDuration) => fedDurations[name.GetStableHashCode()] = fedDuration;
        public static bool GetFedDuration(string name, out float fedDuration) => fedDurations.TryGetValue(name.GetStableHashCode(), out fedDuration);



        private static readonly Dictionary<int, float> animationScaling = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> eggScales = new Dictionary<int, float>();
        public static void SetAnimationScaling(string name, float scale) => animationScaling[name.GetStableHashCode()] = scale;
        public static void SetEggScale(string name, float scale) => eggScales[name.GetStableHashCode()] = scale;
        public static bool GetAnimationScaling(string name, out float scale) => animationScaling.TryGetValue(name.GetStableHashCode(), out scale);
        public static float GetEggScale(string name) => eggScales.TryGetValue(name.GetStableHashCode(), out float scale) ? scale : 1;





        private static readonly Dictionary<int, Heightmap.Biome> eggNeedsAnyBiome = new Dictionary<int, Heightmap.Biome>();
        public static void SetEggNeedsAnyBiome(string name, Heightmap.Biome[] biomes)
        {
            Heightmap.Biome mask = Heightmap.Biome.None;
            if (biomes != null)
                for (int i = 0; i < biomes.Length; ++i) mask |= biomes[i];

            eggNeedsAnyBiome[name.GetStableHashCode()] = mask;
        }
        public static Heightmap.Biome GetEggNeedsAnyBiome(string name)
            => eggNeedsAnyBiome.TryGetValue(name.GetStableHashCode(), out var m) ? m : Heightmap.Biome.None;


        public class LiquidInfo
        {
            public Helpers.EnvironmentHelper.LiquidTypeEx Type;
            public float Depth;
        }
        private static readonly Dictionary<int, LiquidInfo> eggNeedsLiquid = new Dictionary<int, LiquidInfo>();
        public static void SetEggNeedsLiquid(string name, Helpers.EnvironmentHelper.LiquidTypeEx type, float depth)
        {
            eggNeedsLiquid[name.GetStableHashCode()] = new LiquidInfo
            {
                Type = type,
                Depth = depth
            };
        }
        public static LiquidInfo GetEggNeedsLiquid(string name)
        {
            if (eggNeedsLiquid.TryGetValue(name.GetStableHashCode(), out LiquidInfo i))
            {
                return i;
            }
            return null;
        }







        public static void Reset()
        {
            canAttackTames.Clear();
            canBeAttackedByTames.Clear();

            stickToFaction.Clear();

            fedDurations.Clear();

            animationScaling.Clear();
            eggScales.Clear();

            eggNeedsAnyBiome.Clear();
        }


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
