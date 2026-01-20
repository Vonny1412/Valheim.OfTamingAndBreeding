using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Handling.Base
{
    interface IDataHandler
    {
        string DirectoryName { get; }
        string ModelTypeName { get; }

        string GetDataKey(string fileName);


        bool LoadFromYaml(string prefabName, string yamlString);
        bool LoadFromFile(string file);
        Dictionary<string, string> GetAllYamlData();
        int GetLoadedDataCount();
        void ResetData();

        void Prepare(DataHandlerContext ctx);
        void ValidateAllData(DataHandlerContext ctx);
        void PrepareAllPrefabs(DataHandlerContext ctx);
        bool ValidateAllPrefabs(DataHandlerContext ctx);
        void RegisterAllPrefabs(DataHandlerContext ctx);
        void Cleanup(DataHandlerContext ctx);
        void RestoreAllPrefabs(DataHandlerContext ctx);

    }
}
