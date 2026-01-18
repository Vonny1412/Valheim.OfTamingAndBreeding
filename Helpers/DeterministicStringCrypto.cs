using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OfTamingAndBreeding.Utils
{
    internal static class DeterministicStringCrypto
    {
        private const byte Version = 1;
        private const int TagLen = 32; // HMACSHA256

        // Format (binary, then Base64):
        // [1 byte version]
        // [ciphertext...]
        // [32 bytes HMACSHA256 over (version + ciphertext)]
        //
        // IV is NOT stored; it's deterministically derived from the key (constant per key).
        // Same plaintext+key => same ciphertext.

        public static string EncryptToBase64(string plaintext, string privateKey)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("privateKey is required", nameof(privateKey));

            byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);

            byte[] aesKey, hmacKey, iv;
            DeriveKeysAndIv(privateKey, out aesKey, out hmacKey, out iv);

            byte[] cipherBytes = AesCbcEncrypt(plainBytes, aesKey, iv);

            byte[] payload;
            using (var ms = new MemoryStream())
            {
                ms.WriteByte(Version);
                ms.Write(cipherBytes, 0, cipherBytes.Length);
                payload = ms.ToArray();
            }

            byte[] tag;
            using (var hmac = new HMACSHA256(hmacKey))
            {
                tag = hmac.ComputeHash(payload);
            }

            byte[] final = new byte[payload.Length + TagLen];
            Buffer.BlockCopy(payload, 0, final, 0, payload.Length);
            Buffer.BlockCopy(tag, 0, final, payload.Length, TagLen);

            Wipe(aesKey);
            Wipe(hmacKey);
            Wipe(iv);

            return Convert.ToBase64String(final);
        }

        public static string DecryptFromBase64(string base64, string privateKey)
        {
            if (base64 == null) throw new ArgumentNullException(nameof(base64));
            if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("privateKey is required", nameof(privateKey));

            byte[] all = Convert.FromBase64String(base64);
            if (all.Length < 1 + TagLen + 1)
                throw new CryptographicException("Ciphertext payload too small.");

            int payloadLen = all.Length - TagLen;

            byte[] payload = new byte[payloadLen];
            byte[] tag = new byte[TagLen];
            Buffer.BlockCopy(all, 0, payload, 0, payloadLen);
            Buffer.BlockCopy(all, payloadLen, tag, 0, TagLen);

            if (payload[0] != Version)
                throw new CryptographicException("Unsupported crypto version: " + payload[0]);

            byte[] aesKey, hmacKey, iv;
            DeriveKeysAndIv(privateKey, out aesKey, out hmacKey, out iv);

            // Verify HMAC first (tamper detection)
            byte[] expectedTag;
            using (var hmac = new HMACSHA256(hmacKey))
            {
                expectedTag = hmac.ComputeHash(payload);
            }

            if (!FixedTimeEquals(tag, expectedTag))
                throw new CryptographicException("Invalid key or data has been tampered with (HMAC mismatch).");

            int cipherLen = payloadLen - 1;
            byte[] cipherBytes = new byte[cipherLen];
            Buffer.BlockCopy(payload, 1, cipherBytes, 0, cipherLen);

            byte[] plainBytes = AesCbcDecrypt(cipherBytes, aesKey, iv);

            Wipe(aesKey);
            Wipe(hmacKey);
            Wipe(iv);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static void DeriveKeysAndIv(string privateKey, out byte[] aesKey, out byte[] hmacKey, out byte[] iv)
        {
            // seed = SHA256(utf8(privateKey))
            byte[] seed;
            using (var sha = SHA256.Create())
            {
                seed = sha.ComputeHash(Encoding.UTF8.GetBytes(privateKey));
            }

            // Expand keys via HMAC(seed, label)
            aesKey = HmacExpand(seed, "aes-key", 32);
            hmacKey = HmacExpand(seed, "hmac-key", 32);

            // Deterministic IV from key material (constant per privateKey)
            // Note: For your cache use-case this is fine.
            iv = HmacExpand(seed, "aes-iv", 16);

            Wipe(seed);
        }

        private static byte[] HmacExpand(byte[] key, string label, int bytes)
        {
            byte[] data = Encoding.UTF8.GetBytes(label);
            using (var hmac = new HMACSHA256(key))
            {
                byte[] full = hmac.ComputeHash(data);
                if (bytes == full.Length) return full;

                byte[] outBytes = new byte[bytes];
                Buffer.BlockCopy(full, 0, outBytes, 0, bytes);
                Wipe(full);
                return outBytes;
            }
        }

        private static byte[] AesCbcEncrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var enc = aes.CreateEncryptor())
                {
                    return enc.TransformFinalBlock(plaintext, 0, plaintext.Length);
                }
            }
        }

        private static byte[] AesCbcDecrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var dec = aes.CreateDecryptor())
                {
                    return dec.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
                }
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= (a[i] ^ b[i]);
            return diff == 0;
        }

        private static void Wipe(byte[] data)
        {
            if (data == null) return;
            Array.Clear(data, 0, data.Length);
        }
    }
}
