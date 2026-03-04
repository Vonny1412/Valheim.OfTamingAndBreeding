using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class EggDataContext
    {



        public static readonly List<Data.Models.EggData.EggGrowGrownData[]> grownListByIndex;
        public static readonly HashSet<int> sharedNameHashes;

        static EggDataContext()
        {
            grownListByIndex = new List<Data.Models.EggData.EggGrowGrownData[]>();
            sharedNameHashes = new HashSet<int>();

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                grownListByIndex.Clear();
                sharedNameHashes.Clear();
            });
        }

        public static void RegisterEggSharedName(GameObject egg)
        {
            sharedNameHashes.Add(egg.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.GetStableHashCode());
        }

        public static bool IsRegisteredEggSharedName(string sharedName)
        {
            return sharedNameHashes.Contains(sharedName.GetStableHashCode());
        }

    }
}
