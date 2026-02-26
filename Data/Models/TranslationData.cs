using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
