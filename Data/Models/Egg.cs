using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    [CanBeNull]
    internal class Egg : DataBase<Egg>
    {

        public const string DirectoryName = "Eggs";

        public CloneData Clone = null;
        public ItemData Item = null;
        public EggGrowData EggGrow = null;

        [Serializable]
        [CanBeNull]
        internal class CloneData : SubData.ICloneData
        {
            public string From { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ItemData
        {
            public string Name { get; set; } = null;
            public string Description { get; set; } = null;
            public string ItemType { get; set; } = null;
            public float? Weight { get; set; } = null;

            public float? Scale { get; set; } = null;
            public float? ScaleByQuality { get; set; } = null;
            public float? ScaleWeightByQuality { get; set; } = null;

            public int? MaxQuality { get; set; } = null;
            public int? Value { get; set; } = null;
            public bool? Teleportable { get; set; } = null;
            public int? MaxStackSize { get; set; } = null;

            public int[] ItemTintRgb { get; set; } = null;
            public int[] ParticlesTintRgb { get; set; } = null;
            public int[] LightsTintRgb { get; set; } = null;
            public float LightsScale { get; set; } = 1;
            public bool DisableParticles { get; set; } = false;
        }

        [Serializable]
        [CanBeNull]
        public class EggGrowGrownData : SubData.IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;
            public bool Tamed { get; set; } = true;
            public bool ShowHatchEffect { get; set; } = true;
        }

        [Serializable]
        [CanBeNull]
        public class EggGrowData
        {
            public float GrowTime { get; set; } = 1800;
            public float UpdateInterval { get; set; } = 15f;

            public bool RequireNearbyFire { get; set; } = true;
            public bool RequireUnderRoof { get; set; } = true;
            public float RequireCoverPercentige { get; set; } = 0.7f;

            public EggGrowGrownData[] Grown { get; set; } = null;
        }

    }
}
