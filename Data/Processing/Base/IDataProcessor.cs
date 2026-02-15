using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Processing.Base
{
    interface IDataProcessor
    {
        string DirectoryName { get; }
        string ModelTypeName { get; }

        string GetDataKey(string fileName);


        bool LoadFromYaml(string prefabName, string yamlString);
        bool LoadFromFile(string file);
        Dictionary<string, string> GetAllYamlData();
        int GetLoadedDataCount();
        void ResetData();

        void Prepare(DataProcessorContext ctx);
        void ValidateAllData(DataProcessorContext ctx);
        void PrepareAllPrefabs(DataProcessorContext ctx);
        bool ValidateAllPrefabs(DataProcessorContext ctx);
        void RegisterAllPrefabs(DataProcessorContext ctx);
        void Cleanup(DataProcessorContext ctx);
        void RestoreAllPrefabs(DataProcessorContext ctx);

    }
}
