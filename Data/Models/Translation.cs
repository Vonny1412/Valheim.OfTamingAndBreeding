using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    [CanBeNull]
    internal class Translation : DataBase<Translation>
    {
        public const string DirectoryName = "Translations";

        public string Language { get; set; }
        public Dictionary<string, string> Translations { get; set; }
    }
}
