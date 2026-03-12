using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.StaticContext
{
    internal class EggDataContext
    {

        [NonSerialized] private static readonly HashSet<int> _sharedNameHashes;

        static EggDataContext()
        {
            _sharedNameHashes = new HashSet<int>();

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                _sharedNameHashes.Clear();
            });
        }

        public static void RegisterEggSharedName(GameObject egg)
        {
            _sharedNameHashes.Add(egg.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.GetStableHashCode());
        }

        public static bool IsRegisteredEggSharedName(string sharedName)
        {
            return _sharedNameHashes.Contains(sharedName.GetStableHashCode());
        }



    }
}
