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
        string PrefabTypeName { get; }

        string GetDataKey(string fileName);

        bool LoadFromYaml(string prefabName, string yamlString);
        bool LoadFromFile(string file);
        Dictionary<string, string> GetAllYamlData();
        int GetLoadedDataCount();
        void ResetData();

        void Orch_Prepare(PrefabRegistry reg);
        void Orch_ValidateAllData(PrefabRegistry reg);
        void Orch_ReserveAllPrefabs(PrefabRegistry reg);
        bool Orch_ValidateAllPrefabs(PrefabRegistry reg);
        void Orch_RegisterAllPrefabs(PrefabRegistry reg);
        void Orch_Finalize(PrefabRegistry reg);
        void Orch_RestoreAllPrefabs(PrefabRegistry reg);
        void Orch_Cleanup(PrefabRegistry reg);

    }
}
