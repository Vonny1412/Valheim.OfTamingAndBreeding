using BaseAI_Alias = BaseAI;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using Growup_Alias = Growup;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class Growup : UnityEngine.MonoBehaviour
    {
        public Growup(Growup_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<BaseAI_Alias> __IAPI_m_baseAI_Invoker = new Core.Invokers.FieldMutateInvoker<BaseAI_Alias>(typeof(Growup_Alias), "m_baseAI");

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(Growup_Alias), "m_nview");

        public static readonly Core.Invokers.TypedMethodInvoker<UnityEngine_GameObject_Alias> __IAPI_GetPrefab_Invoker1 = new Core.Invokers.TypedMethodInvoker<UnityEngine_GameObject_Alias>(typeof(Growup_Alias), "GetPrefab", new Core.Signatures.ParamSig[] { });

    }
}
