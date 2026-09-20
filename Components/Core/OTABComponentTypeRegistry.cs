using System;
using System.Collections.Generic;
using System.Linq;

namespace OfTamingAndBreeding.Components.Core
{
    internal static class OTABComponentTypeRegistry
    {
        private sealed class OTABComponentType
        {
            public Type Type { get; }
            public Type[] RequiredTypes { get; }

            public OTABComponentType(Type type, Type[] requiredTypes)
            {
                Type = type;
                RequiredTypes = requiredTypes;
            }
        }

        private static readonly Dictionary<Type, OTABComponentType> s_registeredTypes = new Dictionary<Type, OTABComponentType>();

        public static void RegisterType(Type type, Type[] requiredTypes)
        {
            s_registeredTypes[type] = new OTABComponentType(type, requiredTypes);
        }

        public static void ForEachRegisteredType(Action<Type> action)
        {
            foreach (var type in s_registeredTypes.Keys)
            {
                action(type);
            }
        }

        public static void AddComponentsToPrefabs()
        {
            foreach (OTABComponentType t in s_registeredTypes.Values)
            {
                foreach (var prefab in ZNetScene.instance.m_prefabs)
                {
                    var component = prefab.GetComponent(t.Type);
                    if (component)
                    {
                        continue;
                    }
                    if (!t.RequiredTypes.All(type => prefab.GetComponent(type)))
                    {
                        continue;
                    }
                    Plugin.LogDebug($"Adding OTABComponent '{t.Type.Name}' to prefab '{prefab.name}'");
                    prefab.AddComponent(t.Type);
                }
            }
        }

        public static void RemoveComponentFromPrefabs(Type t)
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                var addedComponent = prefab.GetComponent(t);
                if (!addedComponent)
                {
                    continue;
                }
                Plugin.LogDebug($"Removing OTABComponent '{t.Name}' from prefab '{prefab.name}'");
                UnityEngine.Object.DestroyImmediate(addedComponent);
            }
        }

        public static void RemoveComponentsFromPrefabs()
        {
            foreach (OTABComponentType t in s_registeredTypes.Values)
            {
                RemoveComponentFromPrefabs(t.Type);
            }
        }

    }
}
