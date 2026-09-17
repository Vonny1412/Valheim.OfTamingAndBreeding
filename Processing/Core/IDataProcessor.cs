using System;
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

        void ResetData();

        void CallPrepareProcess();
        bool CallValidateAllData();
        bool CallReserveAllPrefabs();
        bool CallValidateAllPrefabs();
        void CallRegisterAllPrefabs();
        bool CallEditAllPrefabs();
        void CallFinalizeProcess();
        void CallRestoreAllPrefabs();
        void CallCleanupProcess();

    }
}
