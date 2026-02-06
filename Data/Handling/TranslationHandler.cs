using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jotunn.Entities;
using Jotunn.Managers;

using OfTamingAndBreeding.Data.Handling.Base;
namespace OfTamingAndBreeding.Data.Handling
{
    internal class TranslationHandler : DataHandler<Models.Translation>
    {
        public override string DirectoryName => Models.Translation.DirectoryName;

        private int uniqueFileNumber = 0;
        private CustomLocalization local;

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

        public override void Prepare(DataHandlerContext ctx)
        {
            local = LocalizationManager.Instance.GetLocalization();
        }

        public override bool ValidateData(DataHandlerContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool PreparePrefab(DataHandlerContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override bool ValidatePrefab(DataHandlerContext ctx, string fileName, Models.Translation data)
        {
            return true; // i dont care
        }

        public override void RegisterPrefab(DataHandlerContext ctx, string fileName, Models.Translation data)
        {
            local.AddTranslation(data.Language, data.Translations);
        }

        public override void Cleanup(DataHandlerContext ctx)
        {
            uniqueFileNumber = 0;
            local = null;
        }

        public override void RestorePrefab(DataHandlerContext ctx, string fileName, Models.Translation data)
        {
            // TODO: do i need to unregister localizations?
        }

    }
}
