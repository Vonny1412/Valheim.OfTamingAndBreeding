using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Models.SubData
{
    internal class WeightEntry
    {
        public float weight = 1;

        public static T GetRandom<T>(IReadOnlyList<T> items, Func<T, float> check = null) where T : WeightEntry
        {
            if (items == null || items.Count == 0) return null;
            if (items.Count == 1) return items[0];

            if (check == null) check = (e) => e.weight;

            float total = 0f;
            var weights = new float[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                float w = Mathf.Max(0f, check(items[i]));
                weights[i] = w;
                total += w;
            }

            if (total <= 0f) return null;

            float pick = UnityEngine.Random.value * total;

            float acc = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                acc += weights[i];
                if (pick <= acc) return items[i];
            }

            return items[items.Count - 1];
        }


    }
}
