using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.ValheimAPI.Core.Invokers
{
    public class PropertyInvoker<R>
    {
        protected readonly PropertyInfo member;
        private readonly Func<object, R> _getter;
        private readonly Action<object, R> _setter;

        public PropertyInvoker(Type type, string name)
        {
            var bindingAttr =
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;
            member = type.GetProperty(name, bindingAttr);

            if (member == null)
                throw new MissingFieldException(type.FullName, name);
            if (member.CanRead)
                _getter = CreateGetter(member);
            if (member.CanWrite)
                _setter = CreateSetter(member);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Get(object instance)
        {
            if (_getter == null) throw new InvalidOperationException("Property has no getter");
            return _getter(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(object instance, R value)
        {
            if (_setter == null) throw new InvalidOperationException("Property has no setter");
            _setter(instance, value);
        }

        private static Func<object, R> CreateGetter(PropertyInfo prop)
        {
            var getMethod = prop.GetGetMethod(true);
            if (getMethod == null) return null;

            var dm = new DynamicMethod(
                prop.Name + "_Getter",
                typeof(R),
                new[] { typeof(object) },
                prop.DeclaringType,
                true
            );

            var il = dm.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, prop.DeclaringType);
            }

            il.Emit(getMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, getMethod);

            if (prop.PropertyType.IsValueType && typeof(R) == typeof(object))
                il.Emit(OpCodes.Box, prop.PropertyType);

            il.Emit(OpCodes.Ret);

            return (Func<object, R>)dm.CreateDelegate(typeof(Func<object, R>));
        }

        private static Action<object, R> CreateSetter(PropertyInfo prop)
        {
            var setMethod = prop.GetSetMethod(true);
            if (setMethod == null) return null;

            var dm = new DynamicMethod(
                prop.Name + "_Setter",
                typeof(void),
                new[] { typeof(object), typeof(R) },
                prop.DeclaringType,
                true
            );

            var il = dm.GetILGenerator();

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, prop.DeclaringType);
            }

            il.Emit(OpCodes.Ldarg_1);

            if (prop.PropertyType.IsValueType && typeof(R) == typeof(object))
                il.Emit(OpCodes.Unbox_Any, prop.PropertyType);

            il.Emit(setMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, setMethod);
            il.Emit(OpCodes.Ret);

            return (Action<object, R>)dm.CreateDelegate(typeof(Action<object, R>));
        }
    }
}
