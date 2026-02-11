using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class MonsterAI
        {
            private static readonly Dictionary<int, ItemDrop> prefabItemDrops = new Dictionary<int, ItemDrop>();
            private static readonly Dictionary<int, Models.Creature.MonsterAIConsumItemData[]> customConsumeItems = new Dictionary<int, Models.Creature.MonsterAIConsumItemData[]>();

            public static void Reset()
            {
                prefabItemDrops.Clear();
                customConsumeItems.Clear();
            }

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

            public static void SetCustomConsumeItems(string name, Models.Creature.MonsterAIConsumItemData[] items)
            {
                customConsumeItems[name.GetStableHashCode()] = items;
            }

            public static bool TryGetCustomConsumeItems(string name, out Models.Creature.MonsterAIConsumItemData[] items)
            {
                return customConsumeItems.TryGetValue(name.GetStableHashCode(), out items);
            }

        }
    }
}
