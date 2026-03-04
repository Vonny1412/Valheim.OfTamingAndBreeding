using System;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Components.Base
{
    public abstract class OTABComponent<T> : MonoBehaviour
        where T : OTABComponent<T>
    {

        public static void AddComponentToPrefabs(params Type[] requiredTypes)
        {
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (!prefab.GetComponent<T>())
                {
                    if (requiredTypes.All((t) => (bool)prefab.GetComponent(t)))
                    {
                        Plugin.LogServerDebug($"Adding OTABComponent '{nameof(T)}' to prefab '{prefab.name}'");
                        prefab.AddComponent<T>();
                        OTABComponentRegistry.registeredTypes.Add(typeof(T));
                    }
                }
            }
        }

    }
}
