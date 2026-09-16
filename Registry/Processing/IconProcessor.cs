using OfTamingAndBreeding.Data.Files;
using OfTamingAndBreeding.Data.Files.SubData;
using OfTamingAndBreeding.OTABUtils;
using System;
using System.IO;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class IconProcessor : Base.DataProcessor<IconFile>
    {
        public override string DirectoryName => IconFile.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath) => null;

        public override bool LoadFromFile(string filePath)
        {
            var iconName = Path.GetFileNameWithoutExtension(filePath);
            var fileNameParsed = GetDataKey(filePath);
            if (fileNameParsed != null)
            {
                // file name => prefab name => data key
                iconName = fileNameParsed;
            }

            IconType iconType = IconType.Unknown;
            var iconExt = Path.GetExtension(filePath).ToLower();
            switch (iconExt)
            {
                case ".png":
                    iconType = IconType.Png;
                    break;
            }
            if (iconType == IconType.Unknown)
            {
                Plugin.LogError($"{nameof(IconProcessor)}.{nameof(LoadFromFile)}: Invalid icon extension '{iconExt}' for icon '{iconName}'");
                return false;
            }

            var bytes = File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(bytes);
            var iconData = new IconFile
            {
                Type = iconType,
                Data = base64
            };

            return LoadYamlData(iconName, iconData.Serialize());
        }

        //
        //
        //

        public override void PrepareProcess()
        {
        }

        public override bool ValidateData(string iconName, IconFile data)
        {
            var model = $"{nameof(IconFile)}.{iconName}";
            if (SpriteUtils.TryLoadValidImage(data.Data, out var texture))
            {
                StaticContext.IconDataContext.iconTextures.Add(iconName, texture);
                return true;
            }
            Plugin.LogError($"{nameof(model)}.{nameof(ValidateData)}: Invalid image data for icon '{iconName}'");
            return false;
        }

        public override bool ReservePrefab(string iconName, IconFile data)
        {
            return true;
        }

        public override bool ValidatePrefab(string iconName, IconFile data)
        {
            return true;
        }

        public override void RegisterPrefab(string iconName, IconFile data)
        {
        }

        public override void EditPrefab(string iconName, IconFile data)
        {
        }

        public override void FinalizeProcess()
        {
        }

        public override void RestorePrefab(string iconName)
        {
        }

        public override void CleanupProcess()
        {
        }

    }
}
