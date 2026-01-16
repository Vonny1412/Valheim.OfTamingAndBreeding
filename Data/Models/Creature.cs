using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Models
{

    internal class Creature : DataBase<Creature>
    {

        public static string GetDirectoryName() => "Creatures";


        public CharacterAIData Character = null;
        public MonsterAIData MonsterAI = null;
        public TameableData Tameable = null;
        public ProcreationData Procreation = null;
        
        public class CharacterAIData : SubData.BaseCharacterData
        {
            public bool attacksTames = false;
        }

        public class MonsterAIConsumItemData
        {
            public string prefab = null;
            public float fedDurationMultiply = 1f;
        }

        public class MonsterAIData
        {
            public MonsterAIConsumItemData[] consumeItems = null;
            public float consumeRange = 1;
            public float consumeSearchRange = 10;
            public float consumeSearchInterval = 10;
        }

        public class TameableData
        {
            public float fedDuration = 30f;
            public float tamingTime = 1800f;
            public bool commandable = false;
        }

        public class ProcreationOffspringData : SubData.WeightEntry
        {
            public string prefab = null;
            public bool needPartner = true;
            public string needPartnerPrefab = null;
            //public string needFoodPrefab = null;
            public int maxCreatures = 3;
            public float levelUpChance = 0;
            public int maxLevel = 3;
        }

        public class ProcreationPartnerData : SubData.WeightEntry
        {
            public string prefab = null;
        }

        public class ProcreationData
        {
            public float updateInterval = 10;
            public float totalCheckRange = 10;

            public ProcreationPartnerData[] partner = null;
            public float partnerRecheckSeconds = 60;
            public float partnerCheckRange = 3;
            public int requiredLovePoints = 3;

            public float pregnancyChance = 0.5f;
            public float pregnancyDuration = 60;
            public float spawnOffset = 2f;
            public float spawnOffsetMax = 0;
            public bool spawnRandomDirection = false;

            public bool procreateWhileSwimming = true;

            public float extraOffspringChance = 0.0f;
            public int maxOffspringsPerPregnancy = 0;

            public ProcreationOffspringData[] offspring = null;

        }



    }
}
