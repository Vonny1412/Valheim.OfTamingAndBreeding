using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ObjectDBExtensions
    {


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, GameObject> GetItemsByHash(this ObjectDB odb)
            => ValheimAPI.ObjectDB.__IAPI_m_itemByHash_Invoker.Get(odb);

    }
}
