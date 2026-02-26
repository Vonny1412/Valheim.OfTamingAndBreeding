using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class CacheFile : SerializeableData
    {
        public string ModVersion;
        public string CacheFileName;
        public Dictionary<string, Dictionary<string, string>> Data;
    }
}
