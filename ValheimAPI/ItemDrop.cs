using ItemDrop_Alias = ItemDrop;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class ItemDrop : UnityEngine.MonoBehaviour
    {
        public ItemDrop(ItemDrop_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<ItemDrop_Alias>> __IAPI_s_instances_Invoker = new Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<ItemDrop_Alias>>(typeof(ItemDrop_Alias), "s_instances");

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(ItemDrop_Alias), "m_nview");

    }
}
