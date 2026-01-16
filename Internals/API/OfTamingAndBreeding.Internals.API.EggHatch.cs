
using EggHatch_Alias = EggHatch;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class EggHatch : UnityEngine.MonoBehaviour
    {

        public EggHatch(EggHatch_Alias instance) : base(instance)
        {
        }


        public static readonly VoidMethodInvoker __IAPI_Hatch_Invoker1 = new VoidMethodInvoker(typeof(EggHatch_Alias), "Hatch", new ParamSig[] { });
        public void Hatch() => __IAPI_Hatch_Invoker1.Invoke(((EggHatch_Alias)__IAPI_instance), new object[] { });

        public static readonly VoidMethodInvoker __IAPI_CheckSpawn_Invoker1 = new VoidMethodInvoker(typeof(EggHatch_Alias), "CheckSpawn", new ParamSig[] { });
        public void CheckSpawn() => __IAPI_CheckSpawn_Invoker1.Invoke(((EggHatch_Alias)__IAPI_instance), new object[] { });



    }
}
