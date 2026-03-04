using System.Collections.Generic;

namespace OfTamingAndBreeding.StaticContext
{
    internal static class CreatureDataContext
    {

        public class ConsumeItem
        {
            internal ItemDrop itemDrop;
            internal float fedDurationFactor;
        }

        public static readonly List<ConsumeItem[]> consumeItemData;
        public static readonly List<Data.Models.CreatureData.ProcreationPartnerData[]> partnerData;
        public static readonly List<Data.Models.CreatureData.ProcreationOffspringData[]> offspringData;
        public static readonly List<string[]> maxCreaturesPrefabs;

        static CreatureDataContext()
        {
            consumeItemData = new List<ConsumeItem[]>();
            partnerData = new List<Data.Models.CreatureData.ProcreationPartnerData[]>();
            offspringData = new List<Data.Models.CreatureData.ProcreationOffspringData[]>();
            maxCreaturesPrefabs = new List<string[]>();

            Net.NetworkSessionManager.Instance.OnClosed((dataLoaded) => {
                consumeItemData.Clear();
                partnerData.Clear();
                offspringData.Clear();
                maxCreaturesPrefabs.Clear();
            });
        }

    }
}
