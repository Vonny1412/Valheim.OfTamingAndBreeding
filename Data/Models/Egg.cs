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

        internal class CloneData : SubData.ICloneData
        {
            public string From { get; set; } = null;
        }

        public class ItemData
        {
            public string Name { get; set; } = null;
            public string Description { get; set; } = null;
            public float Weight { get; set; } = 1;

            public float Scale { get; set; } = 1;
            public float ScaleByQuality { get; set; } = 0;
            public float ScaleWeightByQuality { get; set; } = 0;

            public int Value { get; set; } = 0;
            public bool Teleportable { get; set; } = true;
            public int MaxStackSize { get; set; } = 20;

            public int[] ItemTintRgb { get; set; } = null;
            public int[] ParticlesTintRgb { get; set; } = null;
            public int[] LightsTintRgb { get; set; } = null;
            public float LightsScale { get; set; } = 1;
            public bool DisableParticles { get; set; } = false;
        }

        public class EggGrowGrownData : SubData.IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;

            public bool Tamed { get; set; } = true;

            public bool ShowHatchEffect { get; set; } = true;
        }

        public class EggGrowData
        {
            public float GrowTime { get; set; } = 1800;
            public float UpdateInterval { get; set; } = 5;

            public bool RequireNearbyFire { get; set; } = true;
            public bool RequireUnderRoof { get; set; } = true;
            public float RequireCoverPercentige { get; set; } = 0.7f;

            public EggGrowGrownData[] Grown { get; set; } = null;
        }

    }
}
