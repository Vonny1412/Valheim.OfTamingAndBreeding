using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    [CanBeNull]
    internal class Offspring : DataBase<Offspring>
    {

        public const string DirectoryName = "Offsprings";

        public CloneData Clone = null;
        public CharacterData Character = null;
        public GrowupData Growup = null;

        [Serializable]
        [CanBeNull]
        internal class CloneData : SubData.ICloneData
        {
            public string From { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class CharacterData : SubData.ICharacterAIData
        {
            public string Name { get; set; } = null;
            public string Group { get; set; } = null;
            public bool StickToFaction { get; set; } = true;
            public float Scale { get; set; } = 1;
        }

        [Serializable]
        [CanBeNull]
        public class GrowupGrownData : SubData.IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;
        }

        [Serializable]
        [CanBeNull]
        public class GrowupData
        {
            public float GrowTime { get; set; } = 1800;
            public bool InheritTame { get; set; } = true;
            public GrowupGrownData[] Grown { get; set; } = null;
        }

    }
}
