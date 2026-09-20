using System;
using System.Collections.Generic;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Core
{
    public abstract class OTABComponent<T> : MonoBehaviour where T : OTABComponent<T>
    {
        private static readonly Dictionary<GameObject, T> s_cache = new Dictionary<GameObject, T>();

        public static void RegisterType(params Type[] requiredTypes)
        {
            OTABComponentTypeRegistry.RegisterType(typeof(T), requiredTypes);
        }

        protected static void Register(T component)
        {
            s_cache[component.gameObject] = component;
        }

        protected static void Unregister(T component)
        {
            s_cache.Remove(component.gameObject);
        }

        public static T GetUnsafe(GameObject prefab)
        {
            return s_cache[prefab];
        }

        public static bool TryGet(GameObject prefab, out T component)
        {
            return s_cache.TryGetValue(prefab, out component);
        }

        public static T GetOrAddComponent(GameObject prefab)
        {
            var component = prefab.GetComponent<T>();
            if (component)
            {
                return component;
            }
            Plugin.LogDebug($"Adding OTABComponent '{typeof(T).Name}' to prefab '{prefab.name}'");
            component = prefab.AddComponent<T>();
            return component;
        }

    }

}
