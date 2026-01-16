
using System;
using System.Linq;
using System.Reflection;

namespace OfTamingAndBreeding.Internals.API.Core.Invokers
{
    public class TypedMethodInvoker<R> : MethodInvoker
    {
        protected readonly MethodInfo member;
        private readonly MethodInvokerDelegate _invoker;

        public TypedMethodInvoker(Type type, string name, Signatures.ParamSig[] parameters)
        {
            var bindingAttr =
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            Type[] parameterTypes = parameters
                .Select((p)
                    => p.IsByRef ? p.ConcreteType.MakeByRefType() : p.ConcreteType
                )
                .ToArray();

            member = type.GetMethod(
                name,
                bindingAttr,
                binder: null,
                types: parameterTypes,
                modifiers: null
            );

            if (member == null)
                throw new MissingMethodException(type.FullName, name);

            _invoker = CreateInvokerDelegate(member);
        }

        public R Invoke(object instance, object[] args) => (R)_invoker(instance, args);

    }
}
