using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Utilities;
using OfTamingAndBreeding.Processing.Core;
using System;
using System.IO;

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

            TextureType textureType = TextureType.Unknown;
            var textureExtension = Path.GetExtension(filePath).ToLower();
            switch (textureExtension)
            {
                case ".png":
                    textureType = TextureType.Png;
                    break;
            }
            if (textureType == TextureType.Unknown)
            {
                Plugin.LogError($"{nameof(TextureProcessor)}.{nameof(LoadFromFile)}: Invalid extension '{textureExtension}' for texture '{textureName}'");
                return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(bytes);
            var textureData = new TextureFile
            {
                Type = textureType,
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

        public override bool ValidateData(string textureName, TextureFile data)
        {
            var model = $"{nameof(TextureFile)}.{textureName}";
            if (SpriteUtils.TryLoadValidImage(data.Data, out var texture))
            {
                Runtime.TextureDataContext.textures.Add(textureName, texture);
                return true;
            }
            Plugin.LogError($"{nameof(model)}.{nameof(ValidateData)}: Invalid image data for texture '{textureName}'");
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
        }

    }
}
