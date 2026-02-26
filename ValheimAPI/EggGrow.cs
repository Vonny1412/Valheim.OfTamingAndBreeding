using EggGrow_Alias = EggGrow;
using ItemDrop_Alias = ItemDrop;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class EggGrow : UnityEngine.MonoBehaviour
    {
        public EggGrow(EggGrow_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<ItemDrop_Alias> __IAPI_m_item_Invoker = new Core.Invokers.FieldMutateInvoker<ItemDrop_Alias>(typeof(EggGrow_Alias), "m_item");

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(EggGrow_Alias), "m_nview");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_UpdateEffects_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(EggGrow_Alias), "UpdateEffects", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.TypedMethodInvoker<bool> __IAPI_CanGrow_Invoker1 = new Core.Invokers.TypedMethodInvoker<bool>(typeof(EggGrow_Alias), "CanGrow", new Core.Signatures.ParamSig[] { });

    }
}
