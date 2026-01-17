using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Data.Models
{

    [Serializable]
    [CanBeNull]
    internal class Creature : DataBase<Creature>
    {

        public const string DirectoryName = "Creatures";


        public CharacterAIData Character = null;
        public MonsterAIData MonsterAI = null;
        public TameableData Tameable = null;
        public ProcreationData Procreation = null;
        
        public class CharacterAIData : SubData.ICharacterAIData
        {
            public string Group { get; set; } = "";
            public bool StickToFaction { get; set; } = true;
            public bool CanAttackTames { get; set; } = false;
        }

        public class MonsterAIConsumItemData
        {
            public string Prefab { get; set; } = null;
            public float FedDurationMultiply { get; set; } = 1f;
        }

        public class MonsterAIData
        {
            public MonsterAIConsumItemData[] ConsumeItems { get; set; } = null;
            public float ConsumeRange { get; set; } = 1;
            public float ConsumeSearchRange { get; set; } = 10;
            public float ConsumeSearchInterval { get; set; } = 10;
        }

        public class TameableData
        {
            public float FedDuration { get; set; } = 30f;
            public float TamingTime { get; set; } = 1800f;
            public bool Commandable { get; set; } = false;
        }

        public class ProcreationOffspringData : SubData.IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;

            public bool NeedPartner { get; set; } = true;
            public string NeedPartnerPrefab { get; set; } = null;

            public int MaxCreatures { get; set; } = 3;

            public float LevelUpChance { get; set; } = 0;
            public int MaxLevel { get; set; } = 3;
        }

        public class ProcreationPartnerData : SubData.IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;
        }

        public class ProcreationData
        {
            public float UpdateInterval { get; set; } = 10;
            public float TotalCheckRange { get; set; } = 10;

            public ProcreationPartnerData[] Partner { get; set; } = null;
            public float PartnerRecheckSeconds { get; set; } = 60;
            public float PartnerCheckRange { get; set; } = 3;
            public int RequiredLovePoints { get; set; } = 3;

            public float PregnancyChance { get; set; } = 0.5f;
            public float PregnancyDuration { get; set; } = 60;

            public float SpawnOffset { get; set; } = 2f;
            public float SpawnOffsetMax { get; set; } = 0;
            public bool SpawnRandomDirection { get; set; } = false;

            public bool ProcreateWhileSwimming { get; set; } = true;

            public float ExtraOffspringChance { get; set; } = 0.0f;
            public int MaxOffspringsPerPregnancy { get; set; } = 0;

            public ProcreationOffspringData[] Offspring { get; set; } = null;

        }

    }
}
