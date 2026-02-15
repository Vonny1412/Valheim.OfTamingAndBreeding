using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class Procreation
        {
            private static Dictionary<int, long> partnerRecheckTicks = new Dictionary<int, long>();

            public static void Reset()
            {
                partnerRecheckTicks.Clear();
            }

            public static void SetPartnerRecheckTicks(string name, float recheckSeconds)
            {
                partnerRecheckTicks[name.GetStableHashCode()] = TimeSpan.FromSeconds(recheckSeconds).Ticks;
            }

            public static long GetPartnerRecheckTicks(string name, long def)
            {
                if (partnerRecheckTicks.TryGetValue(name.GetStableHashCode(), out long ticks))
                {
                    return ticks;
                }
                return def;
            }

        }
    }
}
