using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.ValheimAPI.Lifecycle
{
    internal abstract class ExtraData<C, T>
        where C : Component
        where T : ExtraData<C, T>, new()
    {
        private static readonly ConditionalWeakTable<C, T> instances
            = new ConditionalWeakTable<C, T>();

        public static T GetOrCreate(C component)
        {
            if (component == null)
            {
                return null;
            }
            return instances.GetValue(component, c =>
            {
                CleanupMarks.Mark(c.GetComponent<ZNetView>());
                return new T();
            });
        }

        public static bool TryGet(C component, out T data)
        {
            if (component == null)
            {
                data = null;
                return false;
            }
            return instances.TryGetValue(component, out data);
        }

        public static bool Remove(C component)
        {
            return instances.Remove(component);
        }

    }
}
