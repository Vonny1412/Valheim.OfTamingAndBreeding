using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class ItemData
        {
            private static readonly Dictionary<int, float> customScales = new Dictionary<int, float>();

            public static void Reset()
            {
                customScales.Clear();
            }

            public static void SetCustomScale(string name, float scale)
            {
                customScales[name.GetStableHashCode()] = scale;
            }

            public static float GetCustomScale(string name)
            {
                return customScales.TryGetValue(name.GetStableHashCode(), out float scale) ? scale : 1;
            }

        }
    }
}
