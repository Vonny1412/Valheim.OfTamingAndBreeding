using System;
using System.Collections.Generic;
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

            // fast outs
            if (Mathf.Approximately(factor, 1f)) return colorNormal;
            if (factor >= max) return colorGood;
            if (factor <= min) return colorBad;

            // decide branch
            bool goodSide = factor > 1f;

            // if any needed color is not hex, bail out to target color (no lerp)
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

            return ComputeLerpedHex(colorBad, colorNormal, colorGood, goodSide);
        }

        // ---------------------------------------------
        // Core compute
        // ---------------------------------------------
        private static string ComputeLerpedHex(
            string colorBad,
            string colorNormal,
            string colorGood,
            bool goodSide)
        {
            int packedA = ParseHexPacked(colorNormal);

            int packedB = goodSide
                ? ParseHexPacked(colorGood)
                : ParseHexPacked(colorBad);

            // unpack
            int ar = (packedA >> 16) & 0xFF;
            int ag = (packedA >> 8) & 0xFF;
            int ab = packedA & 0xFF;

            int br = (packedB >> 16) & 0xFF;
            int bg = (packedB >> 8) & 0xFF;
            int bb = packedB & 0xFF;

            // lerp bytes
            int rr = Mathf.Clamp(Mathf.RoundToInt(ar + (br - ar)), 0, 255);
            int rg = Mathf.Clamp(Mathf.RoundToInt(ag + (bg - ag)), 0, 255);
            int rb = Mathf.Clamp(Mathf.RoundToInt(ab + (bb - ab)), 0, 255);

            return $"#{rr:X2}{rg:X2}{rb:X2}";
        }

        // ---------------------------------------------
        // Hex parsing (packed int 0xRRGGBB)
        // ---------------------------------------------

        private static int ParseHexPacked(string hex)
        {
            // expects "#RRGGBB" (validated before)
            int r = (HexVal(hex[1]) << 4) | HexVal(hex[2]);
            int g = (HexVal(hex[3]) << 4) | HexVal(hex[4]);
            int b = (HexVal(hex[5]) << 4) | HexVal(hex[6]);
            return (r << 16) | (g << 8) | b;
        }

        private static int HexVal(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            return 10 + (c - 'A'); // assume A..F
        }

        private static bool IsHexColor(string s)
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
