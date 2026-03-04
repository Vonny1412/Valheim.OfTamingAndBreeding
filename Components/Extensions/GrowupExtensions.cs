using System.Runtime.CompilerServices;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class GrowupExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetPrefab(this Growup that)
            => ValheimAPI.Growup.__IAPI_GetPrefab_Invoker1.Invoke(that);

    }
}
