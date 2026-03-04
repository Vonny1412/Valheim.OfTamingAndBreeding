using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class TranslationData : DataBase<TranslationData>
    {
        public const string DirectoryName = "Translations";

        public string Language { get; set; }
        public Dictionary<string, string> Translations { get; set; }
    }
}
