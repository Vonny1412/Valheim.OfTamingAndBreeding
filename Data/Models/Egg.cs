using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfTamingAndBreeding.Data.Models
{
    internal class Egg : DataBase<Egg>
    {

        public static string GetDirectoryName() => "Eggs";

        public CloneData Clone = null;
        public ItemData Item = null;
        public EggGrowData EggGrow = null;

        internal class CloneData : SubData.BaseCloneData
        {
        }

        public class ItemData
        {
            public string name = null;
            public string description = null;
            public float weight = 1;
            public float scale = 1;
            public float scaleByQuality = 0;
            public float scaleWeightByQuality = 0;
            public int value = 0;
            public bool teleportable = true;
            public int maxStackSize = 20;
            public int[] itemTintRgb = null;
            public int[] particlesTintRgb = null;
            public int[] lightsTintRgb = null;
            public float lightsScale = 1;
            public bool disableParticles = false;
        }

        public class EggGrowGrownData : SubData.WeightEntry
        {
            public string prefab = null;
            public bool tamed = true;
            public bool showHatchEffect = true;
        }

        public class EggGrowData
        {
            public float growTime = 1800;
            public float updateInterval = 5;
            public bool requireNearbyFire = true;
            public bool requireUnderRoof = true;
            public float requireCoverPercentige = 0.7f;
            public EggGrowGrownData[] grown = null;
        }

    }
}
