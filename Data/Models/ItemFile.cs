using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class ItemFile : DataBase<ItemFile>
    {

        public const string DirectoryName = "Items";

        public CloneData Clone = null;
        public ComponentsData Components = new ComponentsData();

        public ItemData Item = null;
        public FloatingData Floating = null;
        public EggGrowData EggGrow = null;

        [Serializable]
        [CanBeNull]
        public class CloneData
        {
            public string From { get; set; } = null;
            public string VisualFrom { get; set; } = null;
            
            public string Name { get; set; } = null;
            public string Description { get; set; } = null;
            public string ItemType { get; set; } = null;

            public float? Scale { get; set; } = null;
            public float? Weight { get; set; } = null;
            public bool? Teleportable { get; set; } = null;

            public string CustomIconName { get; set; } = null;

            public float? ItemHueShift { get; set; } = null;
            public float? ItemSaturationShift { get; set; } = null;
            public float? ItemBrightnessShift { get; set; } = null;

            public float? LightsHueShift { get; set; } = null;
            public float? LightsSaturationShift { get; set; } = null;

            public float? LightsScale { get; set; } = null;

            public bool? DisableParticles { get; set; } = null;

            public string GroundVisual { get; set; }
            public float? GroundVisualScale { get; set; }
            public GroundVisualOffset GroundVisualOffset { get; set; }
        }

        [Serializable]
        [CanBeNull]
        public class GroundVisualOffset
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public UnityEngine.Vector3 ToVector3()
            {
                return new UnityEngine.Vector3(X, Y, Z);
            }
        }

        [Serializable]
        [CanBeNull]
        public class ComponentsData
        {
            public ComponentBehavior Item { get; set; } = ComponentBehavior.Inherit;
            public ComponentBehavior Floating { get; set; } = ComponentBehavior.Inherit;
            public ComponentBehavior EggGrow { get; set; } = ComponentBehavior.Patch;
        }

        [Serializable]
        [CanBeNull]
        public class ItemData
        {
            public int? MaxStackSize { get; set; } = null;
            public int? MaxQuality { get; set; } = null;
            public float? ScaleByQuality { get; set; } = null;
            public float? ScaleWeightByQuality { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class FloatingData
        {
            public float? WaterLevelOffset { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class EggGrowData
        {

            [Serializable]
            [CanBeNull]
            public class GrownData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;
                public bool Tamed { get; set; } = true;
                public bool ShowHatchEffect { get; set; } = true;
            }

            public float? GrowTime { get; set; } = null;
            public float? UpdateInterval { get; set; } = null;

            public bool? RequireNearbyFire { get; set; } = null;
            public bool? RequireUnderRoof { get; set; } = null;
            public float? RequireCoverPercentige { get; set; } = null;

            public Heightmap.Biome[] RequireAnyBiome { get; set; } = null; // OTAB feature
            public Utilities.EnvironmentUtils.LiquidTypeEx? RequireLiquid { get; set; } = null; // OTAB feature
            public float? RequireLiquidDepth { get; set; } = null; // OTAB feature
            public string[] RequireGlobalKeys { get; set; } = null; // OTAB feature

            public GrownData[] Grown { get; set; } = null;
        }

    }
}
