using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class ItemData
        {
            private static readonly Dictionary<int, float> customScales = new Dictionary<int, float>();
            private static readonly HashSet<int> eggSharedNameHashes = new HashSet<int>();

            public static void Reset()
            {
                customScales.Clear();
            }

            public static void SetCustomScale(string name, float scale)
            {
                customScales[name.GetStableHashCode()] = scale;
            }

            public static float GetCustomScale(string name)
            {
                return customScales.TryGetValue(name.GetStableHashCode(), out float scale) ? scale : 1;
            }

            public static void RegisterEggBySharedName(GameObject egg)
            {
                eggSharedNameHashes.Add(egg.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.GetStableHashCode());
            }

            public static bool IsRegisteredEggBySharedName(string sharedName)
            {
                return eggSharedNameHashes.Contains(sharedName.GetStableHashCode());
            }



        }
    }
}
