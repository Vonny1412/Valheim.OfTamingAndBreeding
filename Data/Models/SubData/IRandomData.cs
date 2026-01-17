using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Models.SubData
{

    public interface IRandomData
    {
        float Weight { get; set; }
    }

    internal static class RandomData
    {
        public static bool FindRandom<T>(IReadOnlyList<T> items, out T entry, Func<T, float> check = null) where T : IRandomData
        {
            entry = default;
            if (items == null || items.Count == 0) return false;
            if (items.Count == 1)
            {
                entry = items[0];
                return true;
            }

            if (check == null) check = (e) => e.Weight;

            float total = 0f;
            var weights = new float[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                float w = Mathf.Max(0f, check(items[i]));
                weights[i] = w;
                total += w;
            }

            if (total <= 0f) return default;

            float pick = UnityEngine.Random.value * total;

            float acc = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                acc += weights[i];
                if (pick <= acc)
                {
                    entry = items[i];
                    return true;
                }
            }

            entry = items[items.Count - 1];
            return true;
        }


    }
}
