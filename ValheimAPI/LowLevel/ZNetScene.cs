using ZDO_Alias = ZDO;
using ZNetScene_Alias = ZNetScene;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class ZNetScene : UnityEngine.MonoBehaviour
    {
        public ZNetScene(ZNetScene_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_CreateObjects_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(ZNetScene_Alias), "CreateObjects", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Collections.Generic.List<ZDO_Alias>), false), new Core.Signatures.NonGenericParamSig(typeof(System.Collections.Generic.List<ZDO_Alias>), false) });
        
    }
}
