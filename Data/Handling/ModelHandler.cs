using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

namespace OfTamingAndBreeding.Data.Handling
{

    public class ModelHandlerContext
    {
        public readonly ZNetScene zns;
        public ModelHandlerContext(ZNetScene zns)
        {
            this.zns = zns;
            cache = new Dictionary<string, UnityEngine.GameObject>();
        }

        private readonly Dictionary<string, UnityEngine.GameObject> cache;

        public bool PrefabExists(string prefabName, bool requireRegistered = false)
        {
            if (!requireRegistered && cache.TryGetValue(prefabName, out _))
            {
                return true;
            }
            //if ((bool)zns.GetPrefab(prefabName))
            if ((bool)GetPrefab(prefabName))
            {
                return true;
            }
            return false;
        }

        public void CachePrefab(string prefabName, UnityEngine.GameObject prefab)
        {
            cache[prefabName] = prefab; // overwrite ok
        }

        public UnityEngine.GameObject GetPrefab(string prefabName)
        {
            if (cache.TryGetValue(prefabName, out UnityEngine.GameObject prefab))
            {
                return prefab;
            }
            prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (prefab) return prefab;
            return zns.GetPrefab(prefabName);
        }

    }

    internal abstract class ModelHandler<T> : IModelHandler where T : Data.DataBase<T>
    {

        public abstract string GetDirectoryName();
        public string GetModelTypeName() => typeof(T).Name;

        public bool LoadFromYaml(string prefabName, string yamlString)
        {
            try
            {
                Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from YAML");
                var data = DataBase<T>.Deserialize(yamlString);
                DataBase<T>.Store(prefabName, data);
                return true;
            }
            catch(Exception e)
            {

            }
            return false;
        }

        public bool LoadFromFile(string file)
        {
            var prefabName = Path.GetFileNameWithoutExtension(file);
            Plugin.LogDebug($"Loading {typeof(T).Name} '{prefabName}' from file");
            try
            {
                var yaml = File.ReadAllText(file);
                LoadFromYaml(prefabName, yaml);
                return true;
            }
            catch(Exception e)
            {
                Plugin.LogFatal($"Invalid yaml file: {file}");
            }
            return false;
        }










        public abstract bool ValidateData(ModelHandlerContext ctx, string prefabName, T data);
        public void ValidateAllData(ModelHandlerContext ctx)
        {
            Plugin.LogInfo($"{nameof(ValidateAllData)}: {typeof(T).Name}");
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

        public abstract bool PreparePrefab(ModelHandlerContext ctx, string prefabName, T data);
        public void PrepareAllPrefabs(ModelHandlerContext ctx)
        {
            Plugin.LogInfo($"{nameof(PrepareAllPrefabs)}: {typeof(T).Name}");
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

        public abstract bool ValidatePrefab(ModelHandlerContext ctx, string prefabName, T data);
        public bool ValidateAllPrefabs(ModelHandlerContext ctx)
        {
            Plugin.LogInfo($"{nameof(ValidateAllPrefabs)}: {typeof(T).Name}");
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

        public abstract void RegisterPrefab(ModelHandlerContext ctx, string prefabName, T data);

        public void RegisterAllPrefabs(ModelHandlerContext ctx)
        {
            Plugin.LogInfo($"{nameof(RegisterAllPrefabs)} {typeof(T).Name}");
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

        public Dictionary<string, string> GetYamlDictFromAll()
        {
            var raw = new Dictionary<string, string>();
            foreach (var kv in Data.DataBase<T>.GetAll())
            {
                raw.Add(kv.Key, kv.Value.Serialize());
            }
            return raw;
        }

        public void LoadAllFromYamlDict(Dictionary<string, string> entries)
        {
            foreach (var kv in entries)
            {
                LoadFromYaml(kv.Key, kv.Value);
            }
        }

        public void ResetData()
        {
            Data.DataBase<T>.DropAll();
        }

    }

}
