using JetBrains.Annotations;
using OfTamingAndBreeding.Common;
using OfTamingAndBreeding.Data.Models.SubData;
using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class OffspringData : DataBase<OffspringData>
    {

        public const string DirectoryName = "Offsprings";

        public CloneData Clone = null;
        public ComponentsData Components = new ComponentsData();
        public GrowupData Growup = null;

        [Serializable]
        [CanBeNull]
        internal class CloneData
        {
            public string From { get; set; } = null;
            public string Name { get; set; } = null;
            public float? Scale { get; set; } = null;
            public float? MaxHealthFactor { get; set; } = null;
            public string[] RemoveEffects { get; set; } = null;
            public bool? DebugEffects { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ComponentsData
        {
            public ComponentBehavior Growup { get; set; } = ComponentBehavior.Inherit; // yes, its optional
        }

        [Serializable]
        [CanBeNull]
        public class GrowupData
        {

            [Serializable]
            [CanBeNull]
            public class GrownData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;
            }

            public float? GrowTime { get; set; } = null;
            public bool? InheritTame { get; set; } = null;
            public GrownData[] Grown { get; set; } = null;
        }

    }
}
