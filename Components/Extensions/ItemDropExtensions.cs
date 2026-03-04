using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class ItemDropExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ItemDrop> GetInstances()
            => ValheimAPI.ItemDrop.__IAPI_s_instances_Invoker.Get(null);

    }
}
