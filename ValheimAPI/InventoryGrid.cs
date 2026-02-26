using Inventory_Alias = Inventory;
using InventoryGrid_Alias = InventoryGrid;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class InventoryGrid : UnityEngine.MonoBehaviour
    {
        public InventoryGrid(InventoryGrid_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<Inventory_Alias> __IAPI_m_inventory_Invoker = new Core.Invokers.FieldMutateInvoker<Inventory_Alias>(typeof(InventoryGrid_Alias), "m_inventory");

    }
}
