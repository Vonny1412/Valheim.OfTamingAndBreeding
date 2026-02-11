using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OfTamingAndBreeding.Data.Processing.Base
{
    public class DataProcessorContext
    {
        public readonly ZNetScene zns;
        public DataProcessorContext(ZNetScene zns)
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

        private static readonly Dictionary<string, GameObject> backups = new Dictionary<string, GameObject>();
        private static readonly HashSet<string> clonedNames = new HashSet<string>();

        private readonly Dictionary<string, HashSet<Type>> addedComponents = new Dictionary<string, HashSet<Type>>();

        public void MakeBackup(string prefabName, GameObject original)
        {
            if (backups.ContainsKey(prefabName)) return;
            if (clonedNames.Contains(prefabName)) return; // is clone. this happens when re-joining a world because the clone is cached inside jotunn
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

        public void Restore(string prefabName, Action<GameObject, GameObject> cb)
        {
            var isClone = clonedNames.Contains(prefabName);
            if (isClone)
            {
                // do not destroy, just ignore it because its just a clone
                return;
            }

            // its not a clone, we need to restore it back to default

            if (!backups.TryGetValue(prefabName, out var backup) || !backup)
            {
                // we got no backup data for that prefab
                // hmmm better just return
                // only restore prefabs with existing backups
                return;
            }

            var current = GetPrefab(prefabName);
            if (current == null)
            {
                // for safety: prefab is completly unknown. should not happen but whatever
                return;
            }

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

            // call callback
            cb(backup, current);

            // NEVER clear the backups! use it as our own cache!
            //UnityEngine.Object.DestroyImmediate(backup);
            //backups.Remove(prefabName);
        }

    }
}
