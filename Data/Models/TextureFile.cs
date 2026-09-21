using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class TextureFile : DataBase<TextureFile>
    {
        public const string DirectoryName = "Textures";

        public SubData.TextureType Type { get; set; }
        public string Data { get; set; }
    }
}
