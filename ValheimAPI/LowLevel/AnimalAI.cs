using AnimalAI_Alias = AnimalAI;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class AnimalAI : BaseAI
    {
        public AnimalAI(AnimalAI_Alias instance) : base(instance)
        {
        }

        public static new readonly Core.Invokers.VoidMethodInvoker __IAPI_SetAlerted_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(AnimalAI_Alias), "SetAlerted", new Core.Signatures.ParamSig[] {new Core.Signatures.NonGenericParamSig(typeof(bool), false)});

    }
}
