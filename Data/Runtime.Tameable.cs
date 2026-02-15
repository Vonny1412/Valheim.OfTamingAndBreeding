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

            private static readonly HashSet<int> fedTimerDisabled = new HashSet<int>();
            private static readonly HashSet<int> tamingDisabled = new HashSet<int>();

            private static readonly Dictionary<int, float> baseFedDurations = new Dictionary<int, float>();
            private static readonly Dictionary<int, float> baseTamingTime = new Dictionary<int, float>();

            private static readonly Dictionary<int, float> starvingGraceMul = new Dictionary<int, float>();

            public static void Reset()
            {
                tamingDisabled.Clear();
                fedTimerDisabled.Clear();

                baseFedDurations.Clear();
                baseTamingTime.Clear();

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

            public static void SetIsFedTimerDisabled(string name)
            {
                fedTimerDisabled.Add(name.GetStableHashCode());
            }

            public static bool GetIsFedTimerDisabled(string name)
            {
                if (Plugin.Configs.GlobalFedDurationFactor.Value < 0)
                {
                    return true;
                }
                return fedTimerDisabled.Contains(name.GetStableHashCode());
            }

            public static void SetBaseFedDuration(string name, float fedDuration)
            {
                baseFedDurations[name.GetStableHashCode()] = fedDuration;
            }

            public static bool TryGetBaseFedDuration(string name, out float fedDuration)
            {
                return baseFedDurations.TryGetValue(name.GetStableHashCode(), out fedDuration);
            }

            public static void SetBaseTamingTime(string name, float time)
            {
                baseTamingTime[name.GetStableHashCode()] = time;
            }

            public static bool TryGetBaseTamingTime(string name, out float time)
            {
                return baseTamingTime.TryGetValue(name.GetStableHashCode(), out time);
            }

            public static void SetStarvingGraceFactor(string name, float mul)
            {
                starvingGraceMul[name.GetStableHashCode()] = mul;
            }

            public static float GetStarvingGraceFactor(string name)
            {
                if (!starvingGraceMul.TryGetValue(name.GetStableHashCode(), out float mul))
                {
                    mul = Plugin.Configs.DefaultStarvingGraceFactor.Value;
                    if (mul < 0) mul = 0;
                }
                return mul;
            }

        }
    }
}
