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

        //
        //
        //

        public override void PrepareProcess()
        {
        }


        internal class TextureData
        {
            public Texture2D Texture { get; }
            public Sprite Sprite { get; }

            public TextureData(Texture2D texture, Sprite sprite)
            {
                Texture = texture;
                Sprite = sprite;
            }
        }


        private static readonly Dictionary<string, TextureData> s_textureData = new Dictionary<string, TextureData>();
        public static bool TryGetSprite(string name, out Sprite sprite)
        {
            if (s_textureData.TryGetValue(name, out var data))
            {
                sprite = data.Sprite;
                return true;
            }
            sprite = default;
            return false;
        }

        public override bool ValidateData(string textureName, TextureFile data)
        {
            var model = $"{nameof(TextureFile)}.{textureName}";
            if (SpriteUtils.TryLoadValidImage(data.Data, out var texture))
            {
                var sprite = SpriteUtils.TextureToSprite(texture);
                if (sprite)
                {
                    s_textureData.Add(textureName, new TextureData(texture, sprite));
                    return true;
                }
                UnityEngine.Object.Destroy(texture);
            }
            Plugin.LogError($"{model}.{nameof(ValidateData)}: Invalid image data for texture '{textureName}'");
            return false;
        }

        public override bool ReservePrefab(string textureName, TextureFile data)
        {
            return true;
        }

        public override bool ValidatePrefab(string textureName, TextureFile data)
        {
            return true;
        }

        public override void RegisterPrefab(string textureName, TextureFile data)
        {
        }

        public override bool ProcessPrefab(string textureName, TextureFile data)
        {
            return true;
        }

        public override void FinalizeProcess()
        {
        }

        public override void RestorePrefab(string textureName)
        {
        }

        public override void CleanupProcess()
        {
            foreach (var data in s_textureData.Values)
            {
                if (data.Sprite)
                {
                    UnityEngine.Object.Destroy(data.Sprite);
                }
                if (data.Texture)
                {
                    UnityEngine.Object.Destroy(data.Texture);
                }
            }
            s_textureData.Clear();
        }

    }
}
