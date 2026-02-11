using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class Tameable
        {

            private static readonly HashSet<int> eatingDisabled = new HashSet<int>();
            private static readonly HashSet<int> tamingDisabled = new HashSet<int>();

            private static readonly Dictionary<int, float> baseFedDurations = new Dictionary<int, float>();
            private static readonly Dictionary<int, float> starvingGraceMul = new Dictionary<int, float>();

            public static void Reset()
            {
                tamingDisabled.Clear();
                eatingDisabled.Clear();

                baseFedDurations.Clear();
                starvingGraceMul.Clear();
            }

            public static void SetTamingDisabled(string name)
            {
                tamingDisabled.Add(name.GetStableHashCode());
            }

            public static bool GetTamingDisabled(string name)
            {
                return tamingDisabled.Contains(name.GetStableHashCode());
            }

            public static void SetIsEatingDisabled(string name)
            {
                eatingDisabled.Add(name.GetStableHashCode());
            }

            public static bool GetIsEatingDisabled(string name)
            {
                return eatingDisabled.Contains(name.GetStableHashCode());
            }

            public static void SetBaseFedDuration(string name, float fedDuration)
            {
                baseFedDurations[name.GetStableHashCode()] = fedDuration;
            }

            public static bool TryGetBaseFedDuration(string name, out float fedDuration)
            {
                return baseFedDurations.TryGetValue(name.GetStableHashCode(), out fedDuration);
            }

            public static void SetStarvingGraceMultiplier(string name, float mul)
            {
                starvingGraceMul[name.GetStableHashCode()] = mul;
            }

            public static float GetStarvingGraceMultiplier(string name)
            {
                if (!starvingGraceMul.TryGetValue(name.GetStableHashCode(), out float mul))
                {
                    mul = 5f;
                    // todo: add default into configs!
                }
                return mul;
            }

        }
    }
}
