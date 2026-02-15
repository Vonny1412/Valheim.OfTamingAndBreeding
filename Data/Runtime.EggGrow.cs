using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data
{
    internal static partial class Runtime
    {
        public static class EggGrow
        {
            public class LiquidInfo
            {
                public Helpers.EnvironmentHelper.LiquidTypeEx Type;
                public float Depth;
            }

            private static readonly Dictionary<int, Heightmap.Biome> eggNeedsAnyBiome = new Dictionary<int, Heightmap.Biome>();
            private static readonly Dictionary<int, LiquidInfo> eggNeedsLiquid = new Dictionary<int, LiquidInfo>();

            private static readonly Dictionary<int, float> baseGrowTime = new Dictionary<int, float>();

            public static void Reset()
            {
                eggNeedsAnyBiome.Clear();
                eggNeedsLiquid.Clear();

                baseGrowTime.Clear();
            }

            public static void SetBaseGrowTime(string name, float time)
            {
                baseGrowTime[name.GetStableHashCode()] = time;
            }

            public static bool TryGetBaseGrowTime(string name, out float time)
            {
                return baseGrowTime.TryGetValue(name.GetStableHashCode(), out time);
            }

            public static void SetEggNeedsAnyBiome(string name, Heightmap.Biome[] biomes)
            {
                Heightmap.Biome mask = Heightmap.Biome.None;
                if (biomes != null)
                    for (int i = 0; i < biomes.Length; ++i) mask |= biomes[i];

                eggNeedsAnyBiome[name.GetStableHashCode()] = mask;
            }

            public static Heightmap.Biome GetEggNeedsAnyBiome(string name)
            {
                return eggNeedsAnyBiome.TryGetValue(name.GetStableHashCode(), out var m) ? m : Heightmap.Biome.None;
            }

            public static void SetEggNeedsLiquid(string name, Helpers.EnvironmentHelper.LiquidTypeEx type, float depth)
            {
                eggNeedsLiquid[name.GetStableHashCode()] = new LiquidInfo
                {
                    Type = type,
                    Depth = depth
                };
            }

            public static LiquidInfo GetEggNeedsLiquid(string name)
            {
                if (eggNeedsLiquid.TryGetValue(name.GetStableHashCode(), out LiquidInfo i))
                {
                    return i;
                }
                return null;
            }

        }
    }
}
