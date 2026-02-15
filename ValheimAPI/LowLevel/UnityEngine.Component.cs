using UnityEngine_Component_Alias = UnityEngine.Component;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel.UnityEngine
{
    public partial class Component : UnityEngine.Object
    {
        public Component(UnityEngine_Component_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_GetComponentFastPath_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(UnityEngine_Component_Alias), "GetComponentFastPath", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Type), false), new Core.Signatures.NonGenericParamSig(typeof(System.IntPtr), false) });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_GetComponentsForListInternal_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(UnityEngine_Component_Alias), "GetComponentsForListInternal", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Type), false), new Core.Signatures.NonGenericParamSig(typeof(object), false) });

    }
}
