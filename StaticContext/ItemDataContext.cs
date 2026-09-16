using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal class ItemDataContext
    {

        [NonSerialized] private static readonly HashSet<int> _eggSharedNameHashes;

        static ItemDataContext()
        {
            _eggSharedNameHashes = new HashSet<int>();

            Net.NetworkSessionManager.OnSessionClosed += () => {
                _eggSharedNameHashes.Clear();
            };
        }

        public static void RegisterEggSharedName(GameObject item)
        {
            _eggSharedNameHashes.Add(item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.GetStableHashCode());
        }

        public static bool IsRegisteredEggSharedName(string sharedName)
        {
            return _eggSharedNameHashes.Contains(sharedName.GetStableHashCode());
        }



    }
}
