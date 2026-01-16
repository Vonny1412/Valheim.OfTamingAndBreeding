
using AnimalAI_Alias = AnimalAI;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class AnimalAI : BaseAI
    {

        public AnimalAI(AnimalAI_Alias instance) : base(instance)
        {
        }

        public static new readonly VoidMethodInvoker __IAPI_SetAlerted_Invoker1 = new VoidMethodInvoker(typeof(AnimalAI_Alias), "SetAlerted", new ParamSig[] {new NonGenericParamSig(typeof(bool), false)});
        public override void SetAlerted(bool alert) => __IAPI_SetAlerted_Invoker1.Invoke(((AnimalAI_Alias)__IAPI_instance), new object[] { alert });
        
    }
}
