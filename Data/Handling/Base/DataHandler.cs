using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace OfTamingAndBreeding.Data.Handling.Base
{

    internal abstract class DataHandler<T> : IDataHandler where T : DataBase<T>
    {

        public abstract string DirectoryName { get; }

        public string ModelTypeName => typeof(T).Name;

        public bool LoadFromYaml(string prefabName, string yamlString)
        {
            try
            {
                Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from YAML");
                var data = DataBase<T>.Deserialize(yamlString);
                DataBase<T>.Store(prefabName, data);
                return true;
            }
            catch (Exception)
            {
                Plugin.LogFatal($"Failed loading YAML for {typeof(T).Name} '{prefabName}'");
            }
            return false;
        }

        public bool LoadFromFile(string file)
        {
            var prefabName = Path.GetFileNameWithoutExtension(file);
            Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from file");
            var yaml = File.ReadAllText(file);
            return LoadFromYaml(prefabName, yaml);
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
        // context routine
        //---------------------

        public abstract void Prepare(DataHandlerContext ctx);

        public abstract bool ValidateData(DataHandlerContext ctx, string prefabName, T data);
        public void ValidateAllData(DataHandlerContext ctx)
        {
            Plugin.LogDebug($"{nameof(ValidateAllData)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidateData)}: {typeof(T).Name} '{prefabName}'");
                if (!ValidateData(ctx, prefabName, data))
                {
                    DataBase<T>.Drop(prefabName);
                }
            }
        }

        public abstract bool PreparePrefab(DataHandlerContext ctx, string prefabName, T data);
        public void PrepareAllPrefabs(DataHandlerContext ctx)
        {
            Plugin.LogDebug($"{nameof(PrepareAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(PreparePrefab)}: {typeof(T).Name} '{prefabName}'");
                if (!PreparePrefab(ctx, prefabName, data))
                {
                    DataBase<T>.Drop(prefabName);
                }
            }
        }

        public abstract bool ValidatePrefab(DataHandlerContext ctx, string prefabName, T data);
        public bool ValidateAllPrefabs(DataHandlerContext ctx)
        {
            Plugin.LogDebug($"{nameof(ValidateAllPrefabs)}: {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            var allOkay = true;
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(ValidatePrefab)}: {typeof(T).Name} '{prefabName}'");
                allOkay &= ValidatePrefab(ctx, prefabName, data);
            }
            return allOkay;
        }

        public abstract void RegisterPrefab(DataHandlerContext ctx, string prefabName, T data);
        public void RegisterAllPrefabs(DataHandlerContext ctx)
        {
            Plugin.LogDebug($"{nameof(RegisterAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RegisterPrefab)} {typeof(T).Name} '{prefabName}'");
                RegisterPrefab(ctx, prefabName, data);
            }
        }

        public abstract void Cleanup(DataHandlerContext ctx);

        public abstract void RestorePrefab(DataHandlerContext ctx, string prefabName, T data);
        public void RestoreAllPrefabs(DataHandlerContext ctx)
        {
            Plugin.LogDebug($"{nameof(RestoreAllPrefabs)} {typeof(T).Name}");
            var all = DataBase<T>.GetAll();
            var keys = all.Keys.ToList();
            foreach (var prefabName in keys)
            {
                if (!all.TryGetValue(prefabName, out var data))
                    continue;
                Plugin.LogDebug($"{nameof(RestorePrefab)} {typeof(T).Name} '{prefabName}'");
                RestorePrefab(ctx, prefabName, data);
            }
        }

    }

}
