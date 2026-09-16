using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Data.Files
{
    [Serializable]
    internal class TranslationFile : DataBase<TranslationFile>
    {
        public const string DirectoryName = "Translations";

        public string Language { get; set; }
        public Dictionary<string, string> Translations { get; set; }
    }
}
