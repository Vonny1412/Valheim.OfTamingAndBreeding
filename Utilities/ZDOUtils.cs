using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Utilities
{
    internal static class ZDOUtils
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




        public abstract class ZDOData
        {
            protected abstract void Serialize(ZDODataWriter writer);
            protected abstract void Deserialize(ZDODataReader reader);

            public string ToZDOString()
            {
                var writer = new ZDODataWriter();
                Serialize(writer);
                return writer.ToString();
            }

            public void FromZDOString(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                var reader = new ZDODataReader(value);
                Deserialize(reader);
            }
        }

        public sealed class ZDODataWriter
        {
            private readonly List<string> entries =  new List<string>();

            public void Write(string key, string value)
            {
                if (value is null)
                    return;

                entries.Add($"{key}={value}");
            }

            public void Write(string key, int? value)
            {
                if (!value.HasValue)
                    return;

                entries.Add($"{key}={value.Value}");
            }

            public void Write(string key, float? value)
            {
                if (!value.HasValue)
                    return;

                entries.Add(
                    $"{key}={value.Value.ToString(CultureInfo.InvariantCulture)}"
                );
            }

            public void Write(string key, bool? value)
            {
                if (!value.HasValue)
                    return;

                entries.Add($"{key}={(value.Value ? 1 : 0)}");
            }

            public override string ToString()
            {
                return string.Join("|", entries);
            }
        }

        public sealed class ZDODataReader
        {
            private readonly Dictionary<string, string> values = new Dictionary<string, string>();

            public ZDODataReader(string data)
            {
                foreach (string entry in data.Split('|'))
                {
                    int separator = entry.IndexOf('=');

                    if (separator <= 0)
                        continue;

                    string key = entry[..separator];
                    string value = entry[(separator + 1)..];

                    values[key] = value;
                }
            }



            public string ReadString(string key)
            {
                return values.TryGetValue(key, out string value)
                    ? value
                    : null;
            }

            public int? ReadInt(string key)
            {

                if (!values.TryGetValue(key, out string value))
                    return null;

                try
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return null;
                }
            }

            public float? ReadFloat(string key)
            {

                if (!values.TryGetValue(key, out string value))
                    return null;

                try
                {
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return null;
                }
            }

            public bool? ReadBool(string key)
            {
                if (!values.TryGetValue(key, out string value))
                    return null;

                return value switch
                {
                    "1" => true,
                    "0" => false,
                    _ => null
                };
            }


            public string ReadString(string key, string fallback = "")
            {
                return values.TryGetValue(key, out string value)
                    ? value
                    : fallback;
            }

            public int ReadInt(string key, int fallback = 0)
            {
                return values.TryGetValue(key, out string value)
                    && int.TryParse(value, out int result)
                        ? result
                        : fallback;
            }

            public float ReadFloat(string key, float fallback = 0f)
            {
                return values.TryGetValue(key, out string value)
                    && float.TryParse(
                        value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float result)
                            ? result
                            : fallback;
            }

            public bool ReadBool(string key, bool fallback = false)
            {
                if (!values.TryGetValue(key, out string value))
                    return fallback;

                return value switch
                {
                    "1" => true,
                    "0" => false,
                    _ => fallback
                };
            }
        }







    }
}
