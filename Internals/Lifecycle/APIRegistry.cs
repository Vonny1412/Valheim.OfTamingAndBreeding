using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals.Lifecycle
{
    // class not used (yet)
    /*
    internal static class APIRegistry<ValT, ApiT>
        where ValT : UnityEngine.Component
        where ApiT : API.Core.ClassPublicizer, new()
    {
        private static readonly ConditionalWeakTable<ValT, ApiT> instances
            = new ConditionalWeakTable<ValT, ApiT>();

        public static ApiT GetOrCreate(ValT __instance)
        {
            return instances.GetValue(__instance, inst =>
            {
                Lifecycle.CleanupMarks.Mark(inst.GetComponent<ZNetView>());
                // following is equivalent of: new ApiT(inst)
                var a = new ApiT();
                a.__IAPI_SetInstance(inst);
                return a;
            });
        }

        public static bool TryGet(ValT __instance, out ApiT api)
            => instances.TryGetValue(__instance, out api);

        public static void Remove(ValT __instance)
            => instances.Remove(__instance);

    }
    */
}
