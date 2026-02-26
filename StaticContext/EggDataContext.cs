using OfTamingAndBreeding.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal class EggDataContext
    {



        public static readonly List<Data.Models.EggData.EggGrowGrownData[]> grownListByIndex;
        public static readonly HashSet<int> sharedNameHashes;

        static EggDataContext()
        {
            grownListByIndex = new List<Data.Models.EggData.EggGrowGrownData[]>();
            sharedNameHashes = new HashSet<int>();

            RegistryOrchestrator.OnDataReset(() => {
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
