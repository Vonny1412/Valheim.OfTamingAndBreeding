using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Common
{
    internal static class WeightedRandom
    {
        public interface IWeighted
        {
            float Weight { get; }
        }

        public static bool FindRandom<T>(
            IReadOnlyList<T> items,
            out T entry,
            Func<T, float> check = null
        ) where T : IWeighted
        {
            entry = default;
            if (items == null || items.Count == 0) return false;
            if (items.Count == 1) { entry = items[0]; return true; }

            check ??= e => e.Weight;
            float total = 0f;
            bool any = false;

            for (int i = 0; i < items.Count; i++)
            {
                float w = check(items[i]);
                if (w <= 0f) continue;

                any = true;
                total += w;

                // weighted reservoir sampling: select item with probability w/total
                if (UnityEngine.Random.value * total <= w)
                {
                    entry = items[i];
                }
            }

            return any;
        }
    }
}
