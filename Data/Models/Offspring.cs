using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Models
{
    internal class Offspring : DataBase<Offspring>
    {

        public static string GetDirectoryName() => "Offsprings";

        public CloneData Clone = null;
        public CharacterData Character = null;
        public GrowupData Growup = null;

        internal class CloneData : SubData.BaseCloneData
        {
        }

        public class CharacterData : SubData.BaseCharacterData
        {
            public string name = null;
            public float scale = 1;
        }

        public class GrowupGrownData : SubData.WeightEntry
        {
            public string prefab = null;
        }

        public class GrowupData
        {
            public float growTime = 1800;
            public bool inheritTame = true;
            public GrowupGrownData[] grown = null;
        }

    }
}
