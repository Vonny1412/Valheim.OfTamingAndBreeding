using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Runtime
{
    internal class ItemDataContext
    {

        [NonSerialized] private static readonly HashSet<int> _eggSharedNameHashes;

        static ItemDataContext()
        {
            _eggSharedNameHashes = new HashSet<int>();

            Network.NetworkSessionManager.OnSessionClosed += () => {
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
