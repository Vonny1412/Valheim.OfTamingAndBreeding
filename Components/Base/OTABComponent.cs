using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Base
{
    public abstract class OTABComponent<T> : MonoBehaviour
        where T : OTABComponent<T>
    {

        private static readonly Dictionary<GameObject, T> cache = new Dictionary<GameObject, T>();

        internal static void Register(T component)
        {
            cache[component.gameObject] = component;
        }

        internal static void Unregister(T component)
        {
            cache.Remove(component.gameObject);
            //Plugin.LogServerWarning($"traits count for {typeof(T).Name} : {_traits.Count()}");
        }

        public static T GetUnsafe(GameObject prefab)
        {
            return cache[prefab];
        }

        public static bool TryGet(GameObject prefab, out T component)
        {
            return cache.TryGetValue(prefab, out component);
        }

        public static T GetOrAddComponent(GameObject prefab, params Type[] requiredTypes)
        {
            var component = prefab.GetComponent<T>();
            if (!component)
            {
                if (requiredTypes.All((t) => (bool)prefab.GetComponent(t)))
                {
                    Plugin.LogServerDebug($"Adding OTABComponent '{typeof(T).Name}' to prefab '{prefab.name}'");
                    component = prefab.AddComponent<T>();
                    OTABComponentRegistry.registeredTypes.Add(typeof(T));
                }
            }
            return component;
        }

        public static T GetOrAddComponent(GameObject prefab)
        {
            var component = prefab.GetComponent<T>();
            if (!component)
            {
                Plugin.LogServerDebug($"Adding OTABComponent '{typeof(T).Name}' to prefab '{prefab.name}'");
                component = prefab.AddComponent<T>();
                OTABComponentRegistry.registeredTypes.Add(typeof(T));
            }
            return component;
        }

        public static void AddComponentToPrefabs(params Type[] requiredTypes)
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (!prefab.GetComponent<T>())
                {
                    if (requiredTypes.All((t) => (bool)prefab.GetComponent(t)))
                    {
                        Plugin.LogServerDebug($"Adding OTABComponent '{typeof(T).Name}' to prefab '{prefab.name}'");
                        prefab.AddComponent<T>();
                        OTABComponentRegistry.registeredTypes.Add(typeof(T));
                    }
                }
            }
        }

    }

}
