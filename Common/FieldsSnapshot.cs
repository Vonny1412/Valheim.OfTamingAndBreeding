using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OfTamingAndBreeding.Common
{
    internal sealed class FieldsSnapshot<T>
    {
        private static readonly BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        private static readonly FieldInfo[] s_fields = typeof(T).GetFields(FieldFlags);
        private readonly Dictionary<FieldInfo, object> m_values;

        public FieldsSnapshot(T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            m_values = new Dictionary<FieldInfo, object>(s_fields.Length);
            foreach (var field in s_fields)
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
