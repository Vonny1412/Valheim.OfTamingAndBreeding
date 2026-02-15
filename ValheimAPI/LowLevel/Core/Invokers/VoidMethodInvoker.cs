using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.Core.Invokers
{
    public class VoidMethodInvoker : MethodInvoker
    {
        protected readonly MethodInfo member;
        private readonly MethodInvokerDelegate _invoker;
        private readonly bool anyByRef;

        public VoidMethodInvoker(Type type, string name, Signatures.ParamSig[] parameters)
        {

            var bindingAttr =
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            anyByRef = parameters.Any((p) => p.IsByRef);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object[] args) => _invoker(instance, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodInfo GetMethodInfo() => member;

        //--------------------------------
        // PERF: ThreadStatic args buffers to avoid allocations.

        private static readonly object[] _args0 = Array.Empty<object>();
        [ThreadStatic] private static object[] _args1;
        [ThreadStatic] private static object[] _args2;
        [ThreadStatic] private static object[] _args3;
        [ThreadStatic] private static object[] _args4;
        [ThreadStatic] private static object[] _args5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance)
        {
            _invoker(instance, _args0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object arg0)
        {
            if (anyByRef)
            {
                _invoker(instance, new object[1] { arg0 });
                return;
            }
            var args = _args1 ?? (_args1 = new object[1]);
            args[0] = arg0;
            _invoker(instance, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object arg0, object arg1)
        {
            if (anyByRef)
            {
                _invoker(instance, new object[2] { arg0, arg1 });
                return;
            }
            var args = _args2 ?? (_args2 = new object[2]);
            args[0] = arg0;
            args[1] = arg1;
            _invoker(instance, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object arg0, object arg1, object arg2)
        {
            if (anyByRef)
            {
                _invoker(instance, new object[3] { arg0, arg1, arg2 });
                return;
            }
            var args = _args3 ?? (_args3 = new object[3]);
            args[0] = arg0;
            args[1] = arg1;
            args[2] = arg2;
            _invoker(instance, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object arg0, object arg1, object arg2, object arg3)
        {
            if (anyByRef)
            {
                _invoker(instance, new object[4] { arg0, arg1, arg2, arg3 });
                return;
            }
            var args = _args4 ?? (_args4 = new object[4]);
            args[0] = arg0;
            args[1] = arg1;
            args[2] = arg2;
            args[3] = arg3;
            _invoker(instance, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object instance, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            if (anyByRef)
            {
                _invoker(instance, new object[5] { arg0, arg1, arg2, arg3, arg4 });
                return;
            }
            var args = _args5 ?? (_args5 = new object[5]);
            args[0] = arg0;
            args[1] = arg1;
            args[2] = arg2;
            args[3] = arg3;
            args[4] = arg4;
            _invoker(instance, args);
        }

    }
}
