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






        public static bool TryParseColor(string src, out Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(src))
                return false;

            src = src.Trim();

            if (src[0] == '#')
            {
                string hex = src.Substring(1);

                if (hex.Length != 6 && hex.Length != 8)
                    return false;

                if (!TryParseHexByte(hex, 0, out byte r) ||
                    !TryParseHexByte(hex, 2, out byte g) ||
                    !TryParseHexByte(hex, 4, out byte b))
                    return false;

                byte a = 255;
                if (hex.Length == 8)
                {
                    if (!TryParseHexByte(hex, 6, out a))
                        return false;
                }

                color = new Color(
                    r / 255f,
                    g / 255f,
                    b / 255f,
                    a / 255f
                );
                return true;
            }

            string[] parts = src.Split(',');
            if (parts.Length != 3 && parts.Length != 4)
                return false;

            if (!TryParseByte(parts[0], out byte cr) ||
                !TryParseByte(parts[1], out byte cg) ||
                !TryParseByte(parts[2], out byte cb))
                return false;

            byte ca = 255;
            if (parts.Length == 4)
            {
                if (!TryParseByte(parts[3], out ca))
                    return false;
            }

            color = new Color(
                cr / 255f,
                cg / 255f,
                cb / 255f,
                ca / 255f
            );
            return true;
        }

        public static bool TryParseHexByte(string s, int index, out byte value)
        {
            value = 0;
            if (index + 2 > s.Length)
                return false;

            return byte.TryParse(
                s.Substring(index, 2),
                System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture,
                out value
            );
        }

        public static bool TryParseByte(string s, out byte value)
        {
            return byte.TryParse(s.Trim(), out value);
        }

    }
}
