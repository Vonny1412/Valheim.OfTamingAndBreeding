using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{

    internal class GrowupAPI : API.Growup
    {
        private static readonly ConditionalWeakTable<Growup, GrowupAPI> instances
            = new ConditionalWeakTable<Growup, GrowupAPI>();
        public static GrowupAPI GetOrCreate(Growup __instance)
            => instances.GetValue(__instance, (Growup inst) => new GrowupAPI(inst));
        public static bool TryGetAPI(Growup __instance, out GrowupAPI api)
            => instances.TryGetValue(__instance, out api);

        public GrowupAPI(Growup __instance) : base(__instance)
        {
        }


    }

}
