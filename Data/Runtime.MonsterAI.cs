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

            public static readonly List<Models.Creature.MonsterAIConsumItemData[]> consumeItemData = new List<Models.Creature.MonsterAIConsumItemData[]>();
            public static readonly List<Models.Creature.ProcreationPartnerData[]> partnerData = new List<Models.Creature.ProcreationPartnerData[]>();
            public static readonly List<Models.Creature.ProcreationOffspringData[]> offspringData = new List<Models.Creature.ProcreationOffspringData[]>();
            public static readonly List<string[]> maxCreaturesPrefabs = new List<string[]>();

            public static void Reset()
            {
                prefabItemDrops.Clear();

                consumeItemData.Clear();
                partnerData.Clear();
                offspringData.Clear();
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

        }
    }
}
