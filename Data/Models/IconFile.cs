using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class IconFile : DataBase<IconFile>
    {
        public const string DirectoryName = "Icons";

        public SubData.IconType Type { get; set; }
        public string Data { get; set; }
    }
}
