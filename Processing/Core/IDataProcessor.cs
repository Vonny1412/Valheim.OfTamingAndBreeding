using System.Collections.Generic;

namespace OfTamingAndBreeding.Processing.Core
{
    internal interface IDataProcessor
    {
        string DirectoryName { get; }
        string ModelTypeName { get; }
        string PrefabTypeName { get; }

        string GetDataKey(string fileName);
        bool LoadFromFile(string file);
        bool LoadYamlData(string prefabName, string yamlText);

        Dictionary<string, string> GetAllSerializedData();
        int GetLoadedDataCount();

        bool PrepareProcess();
        bool ReserveAllPrefabNames();
        bool ValidateAllData();
        bool RegisterAllPrefabs();
        bool ProcessAllPrefabs();
        bool FinalizeProcess();

        void RestoreAllPrefabs();
        void CleanupProcess();
        void ResetData();

    }
}
