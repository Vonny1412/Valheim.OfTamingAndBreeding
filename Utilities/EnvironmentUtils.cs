using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    public static class EnvironmentUtils
    {

        /*
public enum LiquidType
{
    Water = 0,
    Tar = 1,
    All = 10
}
        */

        [Serializable]
        public enum LiquidTypeEx
        {
            None = 0,
            Water = 1,
            Tar = 2,
        }

        public const float LiquidEpsilon = 0.05f;

        public static bool IsInLiquid(UnityEngine.Vector3 pos, LiquidType type, float depth = 0f)
        {
            float surface = Floating.GetLiquidLevel(pos, waveFactor: 1f, type: type);
            if (surface <= -10000f) return false;
            return pos.y < surface +LiquidEpsilon -depth;
        }

        public static bool IsInWater(UnityEngine.Vector3 pos, float depth = 0f)
            => IsInLiquid(pos, LiquidType.Water, depth);

        public static bool IsInTar(UnityEngine.Vector3 pos, float depth = 0f)
            => IsInLiquid(pos, LiquidType.Tar, depth);

        /*
    public enum Biome
    {
        None = 0,
        Meadows = 1,
        Swamp = 2,
        Mountain = 4,
        BlackForest = 8,
        Plains = 0x10,
        AshLands = 0x20,
        DeepNorth = 0x40,
        Ocean = 0x100,
        Mistlands = 0x200,
        All = 0x37F
    }
        */

        public static bool IsInBiome(UnityEngine.Vector3 pos, Heightmap.Biome biome)
        {
            var b = Heightmap.FindBiome(pos);
            if ((b & biome) != 0)
            {
                return true;
            }
            return false;
        }










        public static string ParseGlobalKey(string rawKey)
        {
            if (string.IsNullOrEmpty(rawKey))
            {
                return null;
            }
            string[] keyParts = rawKey.Split(':');
            switch (keyParts.Length)
            {

                case 1:
                    {
                        var key = keyParts[0].Trim();
                        return key;
                    }

                case 2:
                    {
                        // todo: shall we still support mod-specific keys?
                        var mod = keyParts[0].Trim();
                        var key = keyParts[1].Trim();
                        if (Integrations.ThirdPartyManager.TryGetPluginMetadata(mod, out _))
                        {
                            return key;
                        }
                        break;
                    }
                default:
                    // todo: warning
                    break;

            }
            return null;
        }









    }
}
