using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Helpers
{
    public static class ColorHelper
    {

        public static string GetColorBetween(
            string colorBad,
            string colorNormal,
            string colorGood,
            float factor,
            float min,
            float max)
        {
            // handle degenerate ranges
            if (Mathf.Approximately(min, max))
                return colorNormal;

            // If "normal baseline" is intended to be factor == 1
            // clamp baseline into the [min, max] interval just in case.
            // (optional, but helps if configs use only >1 values)
            float baseline = 1f;

            // fast outs
            if (Mathf.Approximately(factor, baseline)) return colorNormal;
            if (factor >= max) return colorGood;
            if (factor <= min) return colorBad;

            bool goodSide = factor > baseline;

            // If any needed color is not hex, bail out to the target end color (no lerp)
            if (goodSide)
            {
                if (!IsHexColor(colorNormal) || !IsHexColor(colorGood))
                    return colorGood;
            }
            else
            {
                if (!IsHexColor(colorNormal) || !IsHexColor(colorBad))
                    return colorBad;
            }

            // Compute normalized t in [0..1]
            float t;
            if (goodSide)
            {
                // baseline -> max maps to 0..1
                float denom = (max - baseline);
                if (Mathf.Approximately(denom, 0f)) return colorGood;
                t = (factor - baseline) / denom;
            }
            else
            {
                // min -> baseline maps to 1..0 (so we invert)
                float denom = (baseline - min);
                if (Mathf.Approximately(denom, 0f)) return colorBad;
                t = (baseline - factor) / denom;
            }

            t = Mathf.Clamp01(t);

            return goodSide
                ? LerpHex(colorNormal, colorGood, t)
                : LerpHex(colorNormal, colorBad, t);
        }

        public static string LerpHex(string a, string b, float t)
        {
            int packedA = ParseHexPacked(a);
            int packedB = ParseHexPacked(b);

            int ar = (packedA >> 16) & 0xFF;
            int ag = (packedA >> 8) & 0xFF;
            int ab = packedA & 0xFF;

            int br = (packedB >> 16) & 0xFF;
            int bg = (packedB >> 8) & 0xFF;
            int bb = packedB & 0xFF;

            int rr = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(ar, br, t)), 0, 255);
            int rg = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(ag, bg, t)), 0, 255);
            int rb = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(ab, bb, t)), 0, 255);

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
