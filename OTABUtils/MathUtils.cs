using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OfTamingAndBreeding.OTABUtils
{
    internal static class MathUtils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRangeXZ(UnityEngine.Vector3 a, UnityEngine.Vector3 b, float maxRange)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz <= maxRange * maxRange;
        }

    }
}
