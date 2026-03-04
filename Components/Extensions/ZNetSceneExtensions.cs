using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ZNetSceneExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateObjects(this ZNetScene zns, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
            => ValheimAPI.ZNetScene.__IAPI_CreateObjects_Invoker1.Invoke(zns, currentNearObjects, currentDistantObjects);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, GameObject> GetNamedPrefabs(this ZNetScene zns)
            => ValheimAPI.ZNetScene.__IAPI_m_namedPrefabs_Invoker.Get(zns);

    }
}
