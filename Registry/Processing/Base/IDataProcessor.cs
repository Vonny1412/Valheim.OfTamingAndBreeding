using System;
using System.Collections.Generic;

namespace OfTamingAndBreeding.Registry.Processing.Base
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
        void CallValidateAllData();
        void CallReserveAllPrefabs();
        bool CallValidateAllPrefabs();
        void CallRegisterAllPrefabs();
        void CallEditAllPrefabs();
        void CallFinalizeProcess();
        void CallRestoreAllPrefabs();
        void CallCleanupProcess();

    }
}
