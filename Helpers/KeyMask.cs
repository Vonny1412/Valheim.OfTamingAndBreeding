using System;
using System.Text;

namespace OfTamingAndBreeding.Helpers
{
    internal static class KeyMask
    {
        static byte R(byte b, int r, bool l)
            => (byte)(l ? (b << r) | (b >> (8 - r)) : (b >> r) | (b << (8 - r)));

        static byte[] M(string s, int n)
        {
            var a = Encoding.UTF8.GetBytes(s);
            var m = new byte[n];
            uint x = 0xA5B3571F;

            for (int i = 0; i < n; i++)
            {
                x ^= (uint)(a[i % a.Length] << ((i & 3) << 3));
                x = (x << 5) | (x >> 27);
                x += (uint)(i * 31);
                m[i] = (byte)x;
            }
            return m;
        }

        public static string Obfuscate(string v, string s)
        {
            var b = Encoding.UTF8.GetBytes(v);
            var m = M(s, b.Length);

            for (int i = 0; i < b.Length; i++)
                b[i] = (byte)(
                    R((byte)(b[i] ^ m[i]), (i + m[i]) & 7, true)
                    + (m[i] ^ i)
                );

            return Convert.ToBase64String(b);
        }

        public static string Deobfuscate(string v, string s)
        {
            var b = Convert.FromBase64String(v);
            var m = M(s, b.Length);

            for (int i = 0; i < b.Length; i++)
                b[i] = (byte)(
                    R((byte)(b[i] - (m[i] ^ i)), (i + m[i]) & 7, false)
                    ^ m[i]
                );

            return Encoding.UTF8.GetString(b);
        }
    }
}
