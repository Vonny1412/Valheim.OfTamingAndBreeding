using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Data.Processing.Base
{

    internal abstract class DataProcessor<T> : IDataProcessor where T : DataBase<T>
    {

        public abstract string DirectoryName { get; }

        public string ModelTypeName => typeof(T).Name;

        public abstract string PrefabTypeName { get; }

        public abstract string GetDataKey(string fileName);

        public bool LoadFromYaml(string prefabName, string yamlText)
        {
            try
            {
                Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from YAML");
                var data = DataBase<T>.Deserialize(yamlText);
                DataBase<T>.Store(prefabName, data);
                return true;
            }
            catch (YamlException e)
            {
                Plugin.LogFatal(Helpers.YamlHelper.FormatException(
                    e,
                    yamlText,
                    $"Failed loading YAML for {typeof(T).Name} '{prefabName}'"
                ));
            }

            return false;
        }

        public bool LoadFromFile(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var yamlText = File.ReadAllText(file);
            var fileNameParsed = GetDataKey(file);
            if (fileNameParsed != null)
            {
                // file name => prefab name => data key
                fileName = fileNameParsed;
            }
            return LoadFromYaml(fileName, yamlText);
        }

        public Dictionary<string, string> GetAllYamlData()
        {
            var ret = new Dictionary<string, string>();
            foreach (var kv in DataBase<T>.GetAll())
            {
                var prefabName = kv.Key;
                var prefabData = kv.Value;
                var prefabYaml = prefabData.Serialize();
                ret.Add(prefabName, prefabYaml);
            }
            return ret;
        }

        public int GetLoadedDataCount() => DataBase<T>.GetAll().Count;

        public void ResetData()
        {
            DataBase<T>.DropAll();
        }

        //---------------------
        // orchestrator routine
        //---------------------

        private readonly HashSet<string> reservedPrefabNames = new HashSet<string>(); // for current session

        public abstract void Prepare(PrefabRegistry reg);

        public abstract bool ValidateData(PrefabRegistry reg, string prefabName, T data);

        public abstract bool ReservePrefab(PrefabRegistry reg, string prefabName, T data);

        public abstract bool ValidatePrefab(PrefabRegistry reg, string prefabName, T data);

        public abstract void RegisterPrefab(PrefabRegistry reg, string prefabName, T data);

        public abstract void Finalize(PrefabRegistry reg);

        public abstract void RestorePrefab(PrefabRegistry reg, string prefabName);

        public abstract void Cleanup(PrefabRegistry reg);

        //
        // methods called by orchestrator
        //

        public void Orch_Prepare(PrefabRegistry reg)
        {
            Prepare(reg);
        }

        public void Orch_ValidateAllData(PrefabRegistry reg)
        {
            Plugin.LogDebug($"{nameof(Orch_ValidateAllData)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidateData)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (!ValidateData(reg, prefabName, data))
                    {
                        DataBase<T>.Drop(prefabName);
                    }
                }
                catch(Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidateData)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public void Orch_ReserveAllPrefabs(PrefabRegistry reg)
        {
            Plugin.LogDebug($"{nameof(Orch_ReserveAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    if (PrefabTypeName != null)
                    {
                        if (PrefabRegistry.RegisterPrefabType(prefabName, PrefabTypeName, out string registeredTypeName) == false)
                        {
                            Plugin.LogFatal($"Tried to register {typeof(T).Name} '{prefabName}' as type '{PrefabTypeName}' but has already been registered as type '{registeredTypeName}' before by an other OTAB instance. Rename your custom prefab to avoid prefab corruption");
                            DataBase<T>.Drop(prefabName);
                            continue;
                        }
                    }

                    if (reservedPrefabNames.Contains(prefabName))
                    {
                        Plugin.LogError($"{nameof(ReservePrefab)}: {typeof(T).Name} '{prefabName}' already reserved!");
                        DataBase<T>.Drop(prefabName);
                        continue;
                    }

                    if (ReservePrefab(reg, prefabName, data))
                    {
                        reservedPrefabNames.Add(prefabName);
                    }
                    else
                    {
                        // error should be logged in ReservePrefab()
                        DataBase<T>.Drop(prefabName);
                    }
                    
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ReservePrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public bool Orch_ValidateAllPrefabs(PrefabRegistry reg)
        {
            Plugin.LogDebug($"{nameof(Orch_ValidateAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var allOkay = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidatePrefab)}: {typeof(T).Name} '{prefabName}'");
                try
                {
                    allOkay &= ValidatePrefab(reg, prefabName, data);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(ValidatePrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
            return allOkay;
        }

        public void Orch_RegisterAllPrefabs(PrefabRegistry reg)
        {
            Plugin.LogDebug($"{nameof(Orch_RegisterAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RegisterPrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RegisterPrefab(reg, prefabName, data);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RegisterPrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public void Orch_Finalize(PrefabRegistry reg)
        {
            Finalize(reg);
        }

        public void Orch_RestoreAllPrefabs(PrefabRegistry reg)
        {
            Plugin.LogDebug($"{nameof(Orch_RestoreAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RestorePrefab)} {typeof(T).Name} '{prefabName}'");
                try
                {
                    RestorePrefab(reg, prefabName);
                }
                catch (Exception)
                {
                    Plugin.LogFatal($"{ModelTypeName}.{nameof(RestorePrefab)}() '{prefabName}' failed");
                    throw;
                }
            }
        }

        public void Orch_Cleanup(PrefabRegistry reg)
        {
            Cleanup(reg);
            reservedPrefabNames.Clear();
        }



    }

}
