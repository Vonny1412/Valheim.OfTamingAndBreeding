using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OfTamingAndBreeding.Data.Models.SubData;
namespace OfTamingAndBreeding.Data.Models
{

    [Serializable]
    [CanBeNull]
    internal class Creature : DataBase<Creature>
    {

        public const string DirectoryName = "Creatures";

        public ComponentsData Components = new ComponentsData();

        public CharacterAIData Character = null;
        public MonsterAIData MonsterAI = null;
        public TameableData Tameable = null;
        public ProcreationData Procreation = null;

        [Serializable]
        [CanBeNull]
        public class ComponentsData
        {
            public ComponentBehavior Character { get; set; } = ComponentBehavior.Patch; // cannot be removed
            public ComponentBehavior MonsterAI { get; set; } = ComponentBehavior.Patch; // cannot be removed
            public ComponentBehavior Tameable { get; set; } = ComponentBehavior.Inherit;
            public ComponentBehavior Procreation { get; set; } = ComponentBehavior.Inherit;
        }

        [Serializable]
        [CanBeNull]
        public class CharacterAIData
        {
            public string Group { get; set; } = null;
            public string GroupWhenTamed { get; set; } = null;
            public Character.Faction? FactionWhenTamed { get; set; } = null;
            public bool TamesStickToFaction { get; set; } = false; // OTAB feature
            public IsEnemyCondition TamesCanAttackTames { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition TamesCanBeAttackedByTames { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition TamesCanAttackPlayer { get; set; } = IsEnemyCondition.Never; // OTAB feature
        }

        [Serializable]
        [CanBeNull]
        public class MonsterAIConsumItemData
        {
            public string Prefab { get; set; } = null;
            public float FedDurationFactor { get; set; } = 1f; // OTAB feature
        }

        [Serializable]
        [CanBeNull]
        public class MonsterAIData
        {
            public MonsterAIConsumItemData[] ConsumeItems { get; set; } = null;
            public float? ConsumeRange { get; set; } = null;
            public float? ConsumeSearchRange { get; set; } = null;
            public float? ConsumeSearchInterval { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class TameableData
        {
            public float? FedDuration { get; set; } = null;
            public float? TamingTime { get; set; } = null;
            public bool? Commandable { get; set; } = null;
            public float? StarvingGraceFactor { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ProcreationOffspringData : IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;

            public bool NeedPartner { get; set; } = true; // true = vanilla
            public string NeedPartnerPrefab { get; set; } = null; // OTAB feature

            public float? LevelUpChance { get; set; } = null; // OTAB feature
            public int? MaxLevel { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ProcreationPartnerData : IRandomData
        {
            public string Prefab { get; set; } = null;
            public float Weight { get; set; } = 1;
        }

        [Serializable]
        [CanBeNull]
        public class ProcreationData
        {
            public float? UpdateInterval { get; set; } = null;
            public float? TotalCheckRange { get; set; } = null;

            public ProcreationPartnerData[] Partner { get; set; } = null;
            public float? PartnerRecheckSeconds { get; set; } = null; // OTAB feature
            public float? PartnerCheckRange { get; set; } = null;
            public int? RequiredLovePoints { get; set; } = null;

            public float? PregnancyChance { get; set; } = null;
            public float? PregnancyDuration { get; set; } = null;

            public float? SpawnOffset { get; set; } = null;
            public float? SpawnOffsetMax { get; set; } = null;
            public bool? SpawnRandomDirection { get; set; } = null;

            public bool ProcreateWhileSwimming { get; set; } = true; // OTAB feature

            public int? MaxCreatures { get; set; } = null;
            public List<string> MaxCreaturesCountPrefabs { get; set; } = null; // OTAB feature

            public float ExtraSiblingChance { get; set; } = 0.0f; // OTAB feature
            public int MaxSiblingsPerPregnancy { get; set; } = 0; // OTAB feature

            public ProcreationOffspringData[] Offspring { get; set; } = null;

        }

    }
}
