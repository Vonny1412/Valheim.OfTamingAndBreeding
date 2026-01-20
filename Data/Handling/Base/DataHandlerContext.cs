using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Handling.Base
{
    public class DataHandlerContext
    {
        public readonly ZNetScene zns;
        public DataHandlerContext(ZNetScene zns)
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

        public GameObject CreateClonedPrefab(string newName, string cloneFromName)
        {
            clonedNames.Add(newName);
            return PrefabManager.Instance.CreateClonedPrefab(newName, cloneFromName);
        }

        public GameObject CreateClonedItemPrefab(string newName, string cloneFromName)
        {
            clonedNames.Add(newName);
            CustomItem eggCustom = new CustomItem(newName, cloneFromName);
            ItemManager.Instance.AddItem(eggCustom);
            return eggCustom.ItemPrefab;
        }

        private readonly Dictionary<string, GameObject> backups = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, HashSet<Type>> addedComponents = new Dictionary<string, HashSet<Type>>();
        private readonly HashSet<string> clonedNames = new HashSet<string>();

        public void MakeBackup(string prefabName, GameObject original)
        {
            if (backups.ContainsKey(prefabName)) return;
            var backupName = $"OTAB_BACKUP_{prefabName}";
            var backup = PrefabManager.Instance.GetPrefab(backupName);
            if (!backup)
            {
                // CreateClonedPrefab is calling Instantiate!
                backup = PrefabManager.Instance.CreateClonedPrefab(backupName, original);
            }
            backups[prefabName] = backup;
        }

        public T GetOrAddComponent<T>(string prefabName, GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c) return c;

            c = go.AddComponent<T>();
            if (!addedComponents.TryGetValue(prefabName, out var set))
                addedComponents[prefabName] = set = new HashSet<Type>();
            set.Add(typeof(T));

            return c;
        }

        public void DestroyComponentIfExists<T>(string prefabName, GameObject obj) where T : UnityEngine.Object
        {
            T c = obj.GetComponent<T>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
            // backup should not be neccessary
        }

        public GameObject Restore(string prefabName, Action<GameObject, GameObject> cb)
        {
            var isClone = clonedNames.Contains(prefabName);
            if (isClone)
            {
                PrefabManager.Instance.DestroyPrefab(prefabName);
                return null;
            }
            else
            {
                if (!backups.TryGetValue(prefabName, out var backup) || !backup)
                    return null;
                var current = GetPrefab(prefabName);
                if (current != null)
                    return null;

                // remove tracked added components first
                if (addedComponents.TryGetValue(prefabName, out var added))
                {
                    foreach (var type in added)
                    {
                        var comp = current.GetComponent(type);
                        if (comp) UnityEngine.Object.DestroyImmediate(comp);
                    }
                    addedComponents.Remove(prefabName);
                }

                cb(backup, current);

                //UnityEngine.Object.DestroyImmediate(backup);
                //backups.Remove(prefabName);

                return current;
            }
        }

    }
}
