using System;
using UnityEngine;

namespace OfTamingAndBreeding.OTABUtils
{
    internal static class ColorUtils
    {

        public static string GetColorBetween(
            string colorBad,
            string colorNormal,
            string colorGood,
            string colorZero,
            float factor,
            float min,
            float max)
        {
            if (factor == 0f)
                return colorZero;

            if (Mathf.Approximately(min, max))
                return colorNormal;

            const float baseline = 1f;

            if (Mathf.Approximately(factor, baseline))
                return colorNormal;

            if (factor >= max)
                return colorGood;

            if (factor <= min)
                return colorBad;

            bool goodSide = factor > baseline;

            if (goodSide)
            {
                if (!IsHexColor(colorNormal) || !IsHexColor(colorGood))
                    return colorGood;

                float denom = max - baseline;
                if (Mathf.Approximately(denom, 0f))
                    return colorGood;

                float t = Mathf.Clamp01((factor - baseline) / denom);
                return LerpHexFast(colorNormal, colorGood, t);
            }
            else
            {
                if (!IsHexColor(colorNormal) || !IsHexColor(colorBad))
                    return colorBad;

                float denom = baseline - min;
                if (Mathf.Approximately(denom, 0f))
                    return colorBad;

                float t = Mathf.Clamp01((baseline - factor) / denom);
                return LerpHexFast(colorNormal, colorBad, t);
            }
        }

        public static string LerpHexFast(string a, string b, float t)
        {
            int packedA = ParseHexPacked(a);
            int packedB = ParseHexPacked(b);

            int ar = (packedA >> 16) & 0xFF;
            int ag = (packedA >> 8) & 0xFF;
            int ab = packedA & 0xFF;

            int br = (packedB >> 16) & 0xFF;
            int bg = (packedB >> 8) & 0xFF;
            int bb = packedB & 0xFF;

            int rr = ar + (int)((br - ar) * t + 0.5f);
            int rg = ag + (int)((bg - ag) * t + 0.5f);
            int rb = ab + (int)((bb - ab) * t + 0.5f);

            return $"#{rr:X2}{rg:X2}{rb:X2}";
        }

        public static int ParseHexPacked(string hex)
        {
            int r = (HexVal(hex[1]) << 4) | HexVal(hex[2]);
            int g = (HexVal(hex[3]) << 4) | HexVal(hex[4]);
            int b = (HexVal(hex[5]) << 4) | HexVal(hex[6]);
            return (r << 16) | (g << 8) | b;
        }

        public static int HexVal(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            return 10 + (c - 'A');
        }

        public static bool IsHexColor(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 7 || s[0] != '#') return false;
            for (int i = 1; i < 7; i++)
            {
                char c = s[i];
                bool ok =
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F');
                if (!ok) return false;
            }
            return true;
        }

    }
}
