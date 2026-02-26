using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.ValheimAPI.Core.Invokers
{

    public class FieldMutateInvoker<R> : FieldAccessInvoker<R>
    {

        protected readonly Action<object, R> _setter;

        public FieldMutateInvoker(Type type, string name) : base(type, name)
        {
            _setter = CreateSetter(member);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(object instance, R value) => _setter(instance, value);

        protected static Action<object, R> CreateSetter(FieldInfo field)
        {
            var dm = new DynamicMethod(
                field.Name + "_Setter",
                typeof(void),
                new[] { typeof(object), typeof(R) },
                field.DeclaringType,
                true
            );

            var il = dm.GetILGenerator();

            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
            }

            il.Emit(OpCodes.Ldarg_1);

            if (field.FieldType.IsValueType && typeof(R) == typeof(object))
                il.Emit(OpCodes.Unbox_Any, field.FieldType);

            il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            return (Action<object, R>)dm.CreateDelegate(typeof(Action<object, R>));
        }
    }
}
