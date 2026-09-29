using System;
using System.Collections.Generic;
using System.Reflection;

namespace OfTamingAndBreeding.Common
{
    internal sealed class FieldsSnapshot<T>
    {
        private readonly Dictionary<FieldInfo, object> m_values;

        public FieldsSnapshot(T source, bool declaredOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (declaredOnly)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var fields = typeof(T).GetFields(flags);
            m_values = new Dictionary<FieldInfo, object>(fields.Length);
            foreach (var field in fields)
            {
                m_values[field] = field.GetValue(source);
            }
        }

        public void ApplyTo(T target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            foreach (var pair in m_values)
            {
                pair.Key.SetValue(target, pair.Value);
            }
        }
    }
}
