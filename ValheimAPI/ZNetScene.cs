using ZDO_Alias = ZDO;
using ZNetScene_Alias = ZNetScene;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class ZNetScene : UnityEngine.MonoBehaviour
    {
        public ZNetScene(ZNetScene_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_CreateObjects_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(ZNetScene_Alias), "CreateObjects", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(System.Collections.Generic.List<ZDO_Alias>), false), new Core.Signatures.NonGenericParamSig(typeof(System.Collections.Generic.List<ZDO_Alias>), false) });
        public static readonly Core.Invokers.FieldAccessInvoker<System.Collections.Generic.Dictionary<int, UnityEngine_GameObject_Alias>> __IAPI_m_namedPrefabs_Invoker = new Core.Invokers.FieldAccessInvoker<System.Collections.Generic.Dictionary<int, UnityEngine_GameObject_Alias>>(typeof(ZNetScene_Alias), "m_namedPrefabs");

    }
}
