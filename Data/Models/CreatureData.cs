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

            public IsEnemyCondition TamedCanAttackPlayer { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanBeAttackedByPlayer { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanAttackGroup { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanBeAttackedByGroup { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanAttackFaction { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanBeAttackedByFaction { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanAttackTamed { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanBeAttackedByTamed { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanAttackWild { get; set; } = IsEnemyCondition.Default; // OTAB feature
            public IsEnemyCondition TamedCanBeAttackedByWild { get; set; } = IsEnemyCondition.Default; // OTAB feature
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
            // todo: add "ConsumeAnimationAlt" for food with 0 fedduration factor
            public bool TamedStayNearSpawn { get; set; } = false; // otab feature
        }

        [Serializable]
        [CanBeNull]
        public class TameableData
        {
            // todo: add explicite boolean options "TamingEnabled" and "FeedingEnabled" (?)
            public float? FedDuration { get; set; } = null;
            public float? TamingTime { get; set; } = null;
            public bool? TamingBoostEnabled { get; set; } = null;
            // todo: add option for "m_startsTamed"
            public bool? Commandable { get; set; } = null;
            public float? StarvingGraceFactor { get; set; } = null; // OTAB feature
            public string PetCommandText { get; set; } = null; // OTAB feature // todo: needs wiki entry
            public string PetAnswerText { get; set; } = null; // OTAB feature // todo: needs wiki entry
            public bool? ShowPetEffect { get; set; } = null; // OTAB feature // todo: needs wiki entry
            public string[] RequireGlobalKeys { get; set; } = null; // OTAB feature // todo: make this unneccessary and remove it + remove the zdo key
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
            // todo: wiki: RequiredLovePoints can be 0. love points wont be shown in hover text, 1 is still used for procreation logic

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
