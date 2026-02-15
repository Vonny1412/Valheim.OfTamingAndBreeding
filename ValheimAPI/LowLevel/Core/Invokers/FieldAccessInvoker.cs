using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.Core.Invokers
{
    public class FieldAccessInvoker<R>
    {
        protected readonly FieldInfo member;
        protected readonly Func<object, R> _getter;

        public FieldAccessInvoker(Type type, string name)
        {
            var bindingAttr =
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            member = type.GetField(name, bindingAttr);
            if (member == null)
                throw new MissingFieldException(type.FullName, name);

            if (member.IsLiteral)
            {
                // throw error
            }

            _getter = CreateGetter(member);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Get(object instance) => _getter(instance);

        protected static Func<object, R> CreateGetter(FieldInfo field)
        {
            var dm = new DynamicMethod(
                field.Name + "_Getter",
                typeof(R),
                new[] { typeof(object) },
                field.DeclaringType,
                true
            );

            var il = dm.GetILGenerator();

            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
            }

            il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);

            if (field.FieldType.IsValueType && typeof(R) == typeof(object))
                il.Emit(OpCodes.Box, field.FieldType);

            il.Emit(OpCodes.Ret);

            return (Func<object, R>)dm.CreateDelegate(typeof(Func<object, R>));
        }

    }
}
