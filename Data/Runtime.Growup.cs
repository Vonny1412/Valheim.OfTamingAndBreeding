using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class Growup
        {

            private static readonly Dictionary<int, float> baseGrowTime = new Dictionary<int, float>();

            public static void Reset()
            {
                baseGrowTime.Clear();
            }

            public static void SetBaseGrowTime(string name, float time)
            {
                baseGrowTime[name.GetStableHashCode()] = time;
            }

            public static bool TryGetBaseGrowTime(string name, out float time)
            {
                return baseGrowTime.TryGetValue(name.GetStableHashCode(), out time);
            }

        }
    }
}
