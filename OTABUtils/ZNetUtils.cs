using System;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.OTABUtils
{
    internal static class ZNetUtils
    {

        // this helper class is to make sure that zdo's revisions get only increased if neccessary

        // about unsafe overloads:
        // Use only if 'cur' is a cached snapshot of the same ZDO key from earlier in this tick.
        // Avoids an extra zdo.GetX() call.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetInt(ZDO zdo, int key, int value)
        {
            if (zdo.GetInt(key, int.MinValue) != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetInt(ZDO zdo, int key, int value, int cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetLong(ZDO zdo, int key, long value)
        {
            if (zdo.GetLong(key, long.MinValue) != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetLong(ZDO zdo, int key, long value, long cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetString(ZDO zdo, int key, string value)
        {
            if (zdo.GetString(key, null) != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetString(ZDO zdo, int key, string value, string cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SetFloat(ZDO zdo, int key, float value)
        {
            if (zdo.GetFloat(key, 0f) != value) zdo.Set(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SetFloat(ZDO zdo, int key, float value, float cur) // unsafe! use with care
        {
            if (cur != value) zdo.Set(key, value);
            return value;
        }

    }
}
