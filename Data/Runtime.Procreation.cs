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
            private static readonly Dictionary<int, float> basePregnancyDuration = new Dictionary<int, float>();

            public static void Reset()
            {
                partnerRecheckTicks.Clear();
                basePregnancyDuration.Clear();
            }

            public static void SetBasePregnancyDuration(string name, float time)
            {
                basePregnancyDuration[name.GetStableHashCode()] = time;
            }

            public static bool TryGetBasePregnancyDuration(string name, out float time)
            {
                return basePregnancyDuration.TryGetValue(name.GetStableHashCode(), out time);
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
