
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OfTamingAndBreeding.Utils;
using OfTamingAndBreeding.Registry;

namespace OfTamingAndBreeding.Data
{
    internal class CacheManager
    {

        public static string GetCachePath(string serverName)
            => Path.Combine(Plugin.CacheDir, serverName);

        public static string GetCacheYamlFile(string serverName)
            => Path.Combine(Plugin.CacheDir, $"{serverName}.yml");

        public static string GetCacheCryptedFile(string serverName)
            => Path.Combine(Plugin.CacheDir, $"{serverName}.cache");

        public static string BuildCache(string serverName, string encryptKey, bool writeFiles)
            => BuildCache(serverName, encryptKey, writeFiles, out string _);

        public static string BuildCache(string serverName, string encryptKey, bool writeFiles, out string cacheContent)
        {
            cacheContent = null;

            var cacheDebugFilesPath = GetCachePath(serverName);
            var cacheDebugYamlFile = GetCacheYamlFile(serverName);
            var cacheCryptedFile = GetCacheCryptedFile(serverName);

            if (Directory.Exists(cacheDebugFilesPath))
                Directory.Delete(cacheDebugFilesPath, true);

            if (File.Exists(cacheDebugYamlFile))
                File.Delete(cacheDebugYamlFile);

            if (File.Exists(cacheCryptedFile))
                File.Delete(cacheCryptedFile);

            var cacheFile = new Models.CacheFile
            {
                ModVersion = Plugin.Version,
                CacheFileName = Plugin.Configs.CacheFileName.Value,
                Data = new Dictionary<string, Dictionary<string, string>>()
            };


            foreach(var p in RegistryOrchestrator.IterDataProcessors())
            {
                var writeToDir = Path.Combine(cacheDebugFilesPath, p.DirectoryName);
                Directory.CreateDirectory(writeToDir);
                var data = new Dictionary<string, string>();
                foreach (var kv in p.GetAllSerializedData())
                {
                    var prefabName = kv.Key;
                    var prefabYaml = kv.Value;
                    data.Add(prefabName, Base64Encode(prefabYaml));
                    if (writeFiles)
                    {
                        File.WriteAllText(Path.Combine(writeToDir, $"{prefabName}.yml"), prefabYaml);
                    }
                }
                cacheFile.Data.Add(p.DirectoryName, data);
            }

            string yamlCacheContent = cacheContent = SerializeableData.Serialize(cacheFile);
            string cryptedCacheContent = null;
            if (encryptKey != null)
            {
                cryptedCacheContent = cacheContent = DeterministicStringCrypto.EncryptToBase64(cacheContent, encryptKey);
            }

            if (writeFiles)
            {
                File.WriteAllText(cacheDebugYamlFile, yamlCacheContent);
                if (cryptedCacheContent != null)
                {
                    File.WriteAllText(cacheCryptedFile, cryptedCacheContent);
                }
            }

            var hash = ComputeSha256StringHash(cacheContent);
            return hash;
        }

        /*
        public static bool LoadCacheFromFile(string serverName, string encryptKey)
        {
            var cacheCryptedFile = GetCacheCryptedFile(serverName);
            return LoadCacheFromCrypted(File.ReadAllText(cacheCryptedFile), encryptKey);
        }
        */

        public static bool LoadCacheFromCrypted(string crypted, string encryptKey)
        {
            try
            {
                var cacheFilePlain = encryptKey == null ? crypted : DeterministicStringCrypto.DecryptFromBase64(crypted, encryptKey);
                var cacheFile = SerializeableData.Deserialize<Models.CacheFile>(cacheFilePlain);
                RegistryOrchestrator.ResetData();
                var allokay = true;
                foreach(var p in RegistryOrchestrator.IterDataProcessors())
                {
                    foreach (var kv in cacheFile.Data[p.DirectoryName])
                    {
                        var prefab = kv.Key;
                        var data = Base64Decode(kv.Value);
                        allokay &= p.LoadYaml(prefab, data);
                    }
                }
                return allokay;
            }
            catch (Exception ex)
            {
                Plugin.LogFatal($"Unexpected error while loading data from crypted cache: {ex}");
            }
            return false;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ComputeSha256FileHash(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath is required", nameof(filePath));

            using (var fs = File.OpenRead(filePath))
            {
                return ComputeSha256StreamHash(fs);
            }
        }

        public static string ComputeSha256StringHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("content is required", nameof(content));

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var hash = ComputeSha256StreamHash(ms);
                return hash;
            }
        }

        public static string ComputeSha256StreamHash(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                return BytesToHex(hash);
            }
        }

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("x2")); // lowercase hex

            return sb.ToString();
        }

    }
}
