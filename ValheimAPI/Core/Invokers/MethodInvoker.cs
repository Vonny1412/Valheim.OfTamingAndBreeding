using System.Reflection.Emit;
using System.Reflection;

namespace OfTamingAndBreeding.ValheimAPI.Core.Invokers
{
    public abstract class MethodInvoker
    {
        protected delegate object MethodInvokerDelegate(object instance, object[] args);

        protected static MethodInvokerDelegate CreateInvokerDelegate(MethodInfo method)
        {
            var parameters = method.GetParameters();

            var dm = new DynamicMethod(
                name: method.Name + "_Invoker",
                returnType: typeof(object),
                parameterTypes: new[] { typeof(object), typeof(object[]) },
                restrictedSkipVisibility: true
            );

            var il = dm.GetILGenerator();

            // load instance
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, method.DeclaringType);
            }

            // load params
            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);          // args
                il.Emit(OpCodes.Ldc_I4, i);        // index
                il.Emit(OpCodes.Ldelem_Ref);       // args[i]

                var pType = parameters[i].ParameterType;

                if (pType.IsByRef)
                {
                    var elementType = pType.GetElementType();

                    il.Emit(OpCodes.Unbox, elementType);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, pType);
                }
            }

            // call
            il.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);

            // return
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return (MethodInvokerDelegate)dm.CreateDelegate(typeof(MethodInvokerDelegate));
        }

    }
}
