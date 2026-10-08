using System;
using System.ComponentModel;

using Container_Alias = Container;
using DropTable_Alias = DropTable;
using EffectList_Alias = EffectList;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using Humanoid_Alias = Humanoid;
using Inventory_Alias = Inventory;
using ItemDrop_ItemData_Alias = ItemDrop.ItemData;
using Piece_Alias = Piece;
using PlayerStatType_Alias = PlayerStatType;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class Container : UnityEngine.MonoBehaviour
    {
        public Container(Container_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(Container_Alias), "m_nview");

    }
}
