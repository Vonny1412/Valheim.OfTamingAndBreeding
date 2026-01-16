
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using OfTamingAndBreeding.Helpers;

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



        public static string WriteCache(string serverName, string encryptKey)
        {
            var cachePath = GetCachePath(serverName);
            var cacheYamlFile = GetCacheYamlFile(serverName);
            var cacheCryptedFile = GetCacheCryptedFile(serverName);

            if (File.Exists(cacheCryptedFile))
                File.Delete(cacheCryptedFile);

            var saver = new DataSaver();
            saver.AddList(Data.Models.Creature.GetAll());
            saver.AddList(Data.Models.Egg.GetAll());
            saver.AddList(Data.Models.Offspring.GetAll());
            saver.WriteFiles(cachePath);

            var cacheFile = new Data.Cache.CacheFile
            {
                ModVersion = Plugin.Version,
                CacheFileName = Plugin.Configs.CacheFileName.Value,
                Data = new Dictionary<string, Dictionary<string, string>>()
            };

            DataLoader.IterDataHandlers((dh) => {
                var dir = dh.GetDirectoryName();
                var data = new Dictionary<string, string>();
                foreach (var file in Directory.GetFiles(Path.Combine(cachePath, dir)))
                {
                    var prefab = Path.GetFileNameWithoutExtension(file);
                    data.Add(prefab, Base64Encode(File.ReadAllText(file)));
                }
                cacheFile.Data.Add(dir, data);
            });

            var cacheFilePlain = DataBase.Serialize(cacheFile);
            //File.WriteAllText(cacheYamlFile, cacheFilePlain);
            var cacheFileCrypted = DeterministicStringCrypto.EncryptToBase64(cacheFilePlain, encryptKey);
            File.WriteAllText(cacheCryptedFile, cacheFileCrypted);
            var hash = ComputeSha256FileHash(cacheCryptedFile);

            /*
            if (Directory.Exists(cachePath))
                Directory.Delete(cachePath, true);
            if (File.Exists(cacheYamlFile))
                File.Delete(cacheYamlFile);
            */

            return hash;
        }

        public static bool LoadCacheFromFile(string serverName, string encryptKey)
        {
            var cacheCryptedFile = GetCacheCryptedFile(serverName);
            return LoadCacheFromCrypted(File.ReadAllText(cacheCryptedFile), encryptKey);
        }

        public static bool LoadCacheFromCrypted(string crypted, string encryptKey)
        {
            try
            {
                var cacheFilePlain = DeterministicStringCrypto.DecryptFromBase64(crypted, encryptKey);
                var cacheFile = DataBase.Deserialize<Data.Cache.CacheFile>(cacheFilePlain);
                DataLoader.ResetData();
                var allokay = true;
                DataLoader.IterDataHandlers((dh) => {
                    foreach (var kv in cacheFile.Data[dh.GetDirectoryName()])
                    {
                        var prefab = kv.Key;
                        var data = Base64Decode(kv.Value);
                        allokay &= dh.LoadFromYaml(prefab, data);
                    }
                });
                return allokay;
            }
            catch (Exception e)
            {
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
            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
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
