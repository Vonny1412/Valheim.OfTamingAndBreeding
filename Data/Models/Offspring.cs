using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
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
        public ComponentsData Components = new ComponentsData();

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
        public class ComponentsData
        {
            public ComponentBehavior Character { get; set; } = ComponentBehavior.Patch; // cannot be removed
            public ComponentBehavior Growup { get; set; } = ComponentBehavior.Inherit; // yes, its optional
        }

        [Serializable]
        [CanBeNull]
        public class CharacterData
        {
            public string Name { get; set; } = null;
            public string Group { get; set; } = null;
            public bool TamesStickToFaction { get; set; } = false;
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
            public float? GrowTime { get; set; } = null;
            public bool? InheritTame { get; set; } = null;
            public GrowupGrownData[] Grown { get; set; } = null;
        }

    }
}
