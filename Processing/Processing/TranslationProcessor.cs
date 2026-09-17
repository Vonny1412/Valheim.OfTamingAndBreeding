using System;
using System.IO;
using System.Linq;
using Jotunn.Managers;
using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Processing.Core;

namespace OfTamingAndBreeding.Registry.Processing
{
    internal class TranslationProcessor : DataProcessor<TranslationFile>
    {
        public override string DirectoryName => TranslationFile.DirectoryName;

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
                .Trim('_'); // trim if filePath is empty ... '_' looks like a smiley
        }

        public override bool LoadFromFile(string filePath) => LoadFromYamlFile(filePath);

        //
        //
        //
        public override void PrepareProcess()
        {
        }

        public override bool ValidateData(string fileName, TranslationFile data)
        {
            return true; // i dont care
        }

        public override bool ReservePrefab(string fileName, TranslationFile data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(string fileName, TranslationFile data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(string fileName, TranslationFile data)
        {
            var local = LocalizationManager.Instance.GetLocalization();
            local.AddTranslation(data.Language, data.Translations);
        }

        public override bool EditPrefab(string fileName, TranslationFile data)
        {
            return true;
        }

        public override void FinalizeProcess()
        {
        }

        public override void RestorePrefab(string fileName)
        {
            // TODO: do i need to unregister localizations?
        }

        public override void CleanupProcess()
        {
        }

    }
}
