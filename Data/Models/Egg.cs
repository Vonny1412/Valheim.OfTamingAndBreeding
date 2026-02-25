using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
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
        public ComponentsData Components = new ComponentsData();

        public ItemData Item = null;
        public FloatingData Floating = null;
        public EggGrowData EggGrow = null;

        [Serializable]
        [CanBeNull]
        internal class CloneData : ICloneData
        {
            public string From { get; set; } = null;
            public string Name { get; set; } = null;
            public string Description { get; set; } = null;
            public float? Scale { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ComponentsData
        {
            public ComponentBehavior Item { get; set; } = ComponentBehavior.Patch; // cannot be removed
            public ComponentBehavior Floating { get; set; } = ComponentBehavior.Inherit;
            public ComponentBehavior EggGrow { get; set; } = ComponentBehavior.Patch;
        }

        [Serializable]
        [CanBeNull]
        public class ItemData
        {
            public string ItemType { get; set; } = null;
            public float? Weight { get; set; } = null;

            public int? Value { get; set; } = null;
            public bool? Teleportable { get; set; } = null;
            public int? MaxStackSize { get; set; } = null;

            public int? MaxQuality { get; set; } = null;
            public float? ScaleByQuality { get; set; } = null;
            public float? ScaleWeightByQuality { get; set; } = null;

            public string ItemTintRgb { get; set; } = null;
            public string ParticlesTintRgb { get; set; } = null;
            public string LightsTintRgb { get; set; } = null;
            public float LightsScale { get; set; } = 1;
            public bool DisableParticles { get; set; } = false;


        }

        [Serializable]
        [CanBeNull]
        public class FloatingData
        {
            public float WaterLevelOffset { get; set; } = 0.5f; // init value required
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
            public float? GrowTime { get; set; } = null;
            public float? UpdateInterval { get; set; } = null;

            public bool? RequireNearbyFire { get; set; } = null;
            public bool? RequireUnderRoof { get; set; } = null;
            public float? RequireCoverPercentige { get; set; } = null;

            public Heightmap.Biome[] RequireAnyBiome { get; set; } = null; // OTAB feature
            public Helpers.EnvironmentHelper.LiquidTypeEx? RequireLiquid { get; set; } = null; // OTAB feature
            public float? RequireLiquidDepth { get; set; } = null; // OTAB feature

            public EggGrowGrownData[] Grown { get; set; } = null;
        }

    }
}
