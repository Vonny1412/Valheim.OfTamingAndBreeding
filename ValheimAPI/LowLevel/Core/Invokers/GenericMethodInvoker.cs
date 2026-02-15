using System;
using System.Reflection;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.Core.Invokers
{
    public abstract class GenericMethodInvoker : MethodInvoker
    {

        public static MethodInfo FindGenericMethod(
            Type type,
            string name,
            int genericCount,
            Signatures.ParamSig[] signature)
        {
            foreach (var m in type.GetMethods(
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly))
            {
                if (m.Name != name)
                    continue;

                if (!m.IsGenericMethodDefinition)
                    continue;

                if (m.GetGenericArguments().Length != genericCount)
                    continue;

                var ps = m.GetParameters();
                if (ps.Length != signature.Length)
                    continue;

                bool match = true;

                for (int i = 0; i < ps.Length; i++)
                {
                    var p = ps[i];
                    var s = signature[i];

                    if (!s.Matches(p))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return m;
            }

            throw new MissingMethodException(type.FullName, name);
        }

    }
}
