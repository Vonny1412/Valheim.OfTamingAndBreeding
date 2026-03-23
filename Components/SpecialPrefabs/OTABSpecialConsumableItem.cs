using OfTamingAndBreeding.Components.Base;

namespace OfTamingAndBreeding.Components.SpecialPrefabs
{
    public abstract class OTABSpecialConsumableItem : OTABComponent<OTABSpecialConsumableItem>
    {
        public abstract bool Compare(ItemDrop otherItemDrop);
    }
}
