using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.Core.Invokers
{ 
    public class ConstFieldInvoker<R>
    {
        protected readonly FieldInfo member;
        protected readonly Func<object, R> _getter;

        public ConstFieldInvoker(Type type, string name)
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

            if (!member.IsLiteral)
            {
                // throw error
            }

            object value = member.GetRawConstantValue();
            R casted = (R)value;
            _getter = _ => casted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Get(object instance) => _getter(instance);


    }
}
