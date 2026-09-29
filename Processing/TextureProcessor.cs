using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;
using OfTamingAndBreeding.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OfTamingAndBreeding.Processing
{
    internal class TextureProcessor : DataProcessor<TextureFile>
    {
        public override string DirectoryName => TextureFile.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath) => null;

        //------------------------------------------------

        private readonly List<string> otabTextureNames = new List<string>();
        private static readonly Dictionary<string, Sprite> s_spriteCache = new Dictionary<string, Sprite>();

        public static bool TryGetSprite(string name, out Sprite sprite)
        {
            if (s_spriteCache.TryGetValue(name, out sprite))
            {
                return true;
            }
            sprite = default;
            return false;
        }

        //------------------------------------------------

        // todo: check for any string key that would overwrite an existing translation


        public override bool LoadFromFile(string filePath)
        {
            var textureName = Path.GetFileNameWithoutExtension(filePath);
            var fileNameParsed = GetDataKey(filePath);
            if (fileNameParsed != null)
            {
                // file name => prefab name => data key
                textureName = fileNameParsed;
            }

            var textureExtension = Path.GetExtension(filePath).ToLower();
            switch (textureExtension)
            {
                case ".png":
                    // all okay
                    break;
                default:
                    Plugin.LogError($"{nameof(TextureProcessor)}.{nameof(LoadFromFile)}: Invalid extension '{textureExtension}' for texture '{textureName}'");
                    return false;
            }
  
            var bytes = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(bytes);
            var textureData = new TextureFile
            {
                Data = base64
            };

            return LoadYamlData(textureName, textureData.Serialize());
        }

        public override bool PrepareProcess()
        {
            return true;
        }

        public override bool ReservePrefabName(string textureName)
        {
            if (otabTextureNames.Contains(textureName))
            {
                var model = $"{nameof(TextureFile)}.{textureName}";
                Plugin.LogError($"{model}: Texture is already reserved");
                return false;
            }
            otabTextureNames.Add(textureName);
            return true;
        }

        public override bool ValidateData(string textureName, TextureFile data)
        {
            var model = $"{nameof(TextureFile)}.{textureName}";
            if (SpriteUtils.TryLoadValidImage(data.Data, out var texture))
            {
                var sprite = SpriteUtils.TextureToSprite(texture);
                if (sprite)
                {
                    s_spriteCache.Add(textureName, sprite);
                    return true;
                }
                UnityEngine.Object.Destroy(texture);
            }
            Plugin.LogError($"{model}.{nameof(ValidateData)}: Invalid image data for texture '{textureName}'");
            return false;
        }

        public override bool RegisterPrefab(string textureName, TextureFile data)
        {
            return true;
        }

        public override bool ProcessPrefab(string textureName, TextureFile data)
        {
            return true;
        }

        public override bool FinalizeProcess()
        {
            otabTextureNames.Clear();
            return true;
        }

        public override void RestorePrefab(string textureName)
        {
        }

        public override void CleanupProcess()
        {
            foreach (var sprite in s_spriteCache.Values)
            {
                if (!sprite)
                {
                    continue;
                }
                if (sprite.texture)
                {
                    UnityEngine.Object.Destroy(sprite.texture);
                }
                UnityEngine.Object.Destroy(sprite);
            }

            s_spriteCache.Clear();
            otabTextureNames.Clear();
        }

    }
}
