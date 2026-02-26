using OfTamingAndBreeding.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.StaticContext
{
    internal class CreatureDataContext
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

            RegistryOrchestrator.OnDataReset(() => {
                consumeItemData.Clear();
                partnerData.Clear();
                offspringData.Clear();
                maxCreaturesPrefabs.Clear();
            });
        }

    }
}
