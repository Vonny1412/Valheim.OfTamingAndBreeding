using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{
    /*
    internal class BaseAIAPI : API.BaseAI
    {
        private static readonly ConditionalWeakTable<BaseAI, BaseAIAPI> instances
            = new ConditionalWeakTable<BaseAI, BaseAIAPI>();
        public static BaseAIAPI GetOrCreate(BaseAI __instance)
            => instances.GetValue(__instance, (BaseAI inst) => new BaseAIAPI(inst));
        public static bool TryGetAPI(BaseAI __instance, out BaseAIAPI api)
            => instances.TryGetValue(__instance, out api);

        public BaseAIAPI(BaseAI __instance) : base(__instance)
        {
        }

        // class not used yet

    }
    */
}
