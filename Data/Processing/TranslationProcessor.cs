using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jotunn.Entities;
using Jotunn.Managers;

namespace OfTamingAndBreeding.Data.Processing
{
    internal class TranslationProcessor : Base.DataProcessor<Models.Translation>
    {
        public override string DirectoryName => Models.Translation.DirectoryName;

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

        public override void Prepare(Base.DataProcessorContext ctx)
        {
        }

        public override bool ValidateData(Base.DataProcessorContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool PreparePrefab(Base.DataProcessorContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(Base.DataProcessorContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(Base.DataProcessorContext ctx, string fileName, Models.Translation data)
        {
            var local = LocalizationManager.Instance.GetLocalization();
            local.AddTranslation(data.Language, data.Translations);
        }

        public override void Finalize(Base.DataProcessorContext ctx)
        {
        }

        public override void RestorePrefab(Base.DataProcessorContext ctx, string fileName, Models.Translation data)
        {
            // TODO: do i need to unregister localizations?
        }

        public override void Cleanup(Base.DataProcessorContext ctx)
        {
        }

    }
}
