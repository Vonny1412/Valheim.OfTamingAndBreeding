using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Jotunn.Managers;

namespace OfTamingAndBreeding.Data.Processing
{
    internal class TranslationProcessor : Base.DataProcessor<Models.Translation>
    {
        public override string DirectoryName => Models.Translation.DirectoryName;

        public override string PrefabTypeName => null;

        public override string GetDataKey(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            filePath = filePath.Substring(Plugin.ServerDataDir.Length+1); // +1 for slash

            var filePathSplits = filePath.Split(new char[] { '/', '\\' }).ToList();
            filePathSplits.RemoveAt(0); // remove worldname
            filePathSplits.RemoveAt(filePathSplits.Count - 1); // remove filename
            filePathSplits.RemoveAt(filePathSplits.Count - 1); // remove "Translations" dir
            filePath = String.Join("_", filePathSplits);

            return $"{filePath}_{fileName}"
                .Trim('_'); // trim if filePath is empty ... '_' looks like smiley

            //return $"{uniqueFileNumber++:D3}_{fileName}";
        }

        public override void Prepare(Base.PrefabRegistry reg)
        {
        }

        public override bool ValidateData(Base.PrefabRegistry reg, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool ReservePrefab(Base.PrefabRegistry reg, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(Base.PrefabRegistry reg, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(Base.PrefabRegistry reg, string fileName, Models.Translation data)
        {
            var local = LocalizationManager.Instance.GetLocalization();
            local.AddTranslation(data.Language, data.Translations);
        }

        public override void Finalize(Base.PrefabRegistry reg)
        {
        }

        public override void RestorePrefab(Base.PrefabRegistry reg, string fileName)
        {
            // TODO: do i need to unregister localizations?
        }

        public override void Cleanup(Base.PrefabRegistry reg)
        {
        }

    }
}
