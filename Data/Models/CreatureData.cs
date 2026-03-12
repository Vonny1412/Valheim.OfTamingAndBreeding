using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
using System;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class CreatureData : DataBase<CreatureData>
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
            public IsEnemyCondition CanAttackTamed { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanBeAttackedByTamed { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanAttackPlayer { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanBeAttackedByPlayer { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanAttackGroup { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanBeAttackedByGroup { get; set; } = IsEnemyCondition.Never; // OTAB feature
            public IsEnemyCondition CanAttackFaction { get; set; } = IsEnemyCondition.Always; // OTAB feature
            public IsEnemyCondition CanBeAttackedByFaction { get; set; } = IsEnemyCondition.Always; // OTAB feature
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
            public string ConsumeAnimation { get; set; } = null;
            public bool TamedStayNearSpawn { get; set; } = false; // otab feature
        }

        [Serializable]
        [CanBeNull]
        public class TameableData
        {
            public float? TamingTime { get; set; } = null;
            public float? FedDuration { get; set; } = null;
            public bool? Commandable { get; set; } = null;
            public InteractableCondition Interactable { get; set; } = InteractableCondition.Always; // OTAB feature
            public float? StarvingGraceFactor { get; set; } = null; // OTAB feature
            public string[] RequireGlobalKeys { get; set; } = null; // OTAB feature
        }

        [Serializable]
        [CanBeNull]
        public class ProcreationData
        {

            [Serializable]
            [CanBeNull]
            public class PartnerData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;
            }

            [Serializable]
            [CanBeNull]
            public class OffspringData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;

                public bool NeedPartner { get; set; } = true; // true = vanilla
                public string NeedPartnerPrefab { get; set; } = null; // OTAB feature

                public float? LevelUpChance { get; set; } = null; // OTAB feature
                public int? MaxLevel { get; set; } = null;

                public bool SpawnTamed { get; set; } = true; // OTAB feature
            }

            public float? UpdateInterval { get; set; } = null;
            public float? TotalCheckRange { get; set; } = null;

            public PartnerData[] Partner { get; set; } = null;
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
            public string[] MaxCreaturesCountPrefabs { get; set; } = null; // OTAB feature

            public float ExtraSiblingChance { get; set; } = 0.0f; // OTAB feature
            public int MaxSiblingsPerPregnancy { get; set; } = 0; // OTAB feature

            public OffspringData[] Offspring { get; set; } = null;

        }

    }
}
