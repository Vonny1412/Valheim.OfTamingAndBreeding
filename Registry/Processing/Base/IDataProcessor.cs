using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Registry.Processing.Base
{
    internal interface IDataProcessor
    {
        string DirectoryName { get; }
        string ModelTypeName { get; }
        string PrefabTypeName { get; }

        string GetDataKey(string fileName);
        bool LoadFromFile(string file);
        bool LoadYaml(string prefabName, string yamlText);







        Dictionary<string, string> GetAllSerializedData();
        int GetLoadedDataCount();

        void ResetData();

        void PrepareProcess(PrefabRegistry reg);
        void ValidateAllData(PrefabRegistry reg);
        void ReserveAllPrefabs(PrefabRegistry reg);
        bool ValidateAllPrefabs(PrefabRegistry reg);
        void RegisterAllPrefabs(PrefabRegistry reg);
        void FinalizeProcess(PrefabRegistry reg);
        void RestoreAllPrefabs(PrefabRegistry reg);
        void CleanupProcess(PrefabRegistry reg);

    }
}
