using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class TextureFile : DataBase<TextureFile>
    {
        public const string DirectoryName = "Textures";

        public string Data { get; set; }
    }
}
