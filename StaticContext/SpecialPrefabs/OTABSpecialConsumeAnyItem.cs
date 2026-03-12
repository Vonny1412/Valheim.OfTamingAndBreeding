using System;

namespace OfTamingAndBreeding.StaticContext.SpecialPrefabs
{
    public class OTABSpecialConsumeAnyItem : OTABSpecialConsumableItem
    {
        public override bool Compare(ItemDrop otherItemDrop)
        {
            return true;
        }
    }
}
