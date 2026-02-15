
using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Data.Caching
{
    internal class CacheFile : DataBase
    {

        public string ModVersion;
        public string CacheFileName;
        public Dictionary<string, Dictionary<string, string>> Data;

    }
}
