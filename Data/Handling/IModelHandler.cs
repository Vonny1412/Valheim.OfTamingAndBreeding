using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Handling
{
    interface IModelHandler
    {
        string DirectoryName { get; }
        string ModelTypeName { get; }

        Dictionary<string, string> GetAllYamlData();
        int GetLoadedDataCount();

        bool LoadFromYaml(string prefabName, string yamlString);
        bool LoadFromFile(string file);

        void ValidateAllData(ModelHandlerContext ctx);
        void PrepareAllPrefabs(ModelHandlerContext ctx);
        bool ValidateAllPrefabs(ModelHandlerContext ctx);
        void RegisterAllPrefabs(ModelHandlerContext ctx);

        Dictionary<string, string> GetYamlDictFromAll();
        void LoadAllFromYamlDict(Dictionary<string, string> entries);

        void ResetData();

    }
}
