using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class CreatureFile : DataBase<CreatureFile>
    {

        public const string DirectoryName = "Creatures";

        [YamlMember(Order = 1)]
        public CloneData Clone = null;

        [YamlMember(Order = 2)]
        public ComponentsData Components = new ComponentsData();

        [YamlMember(Order = 3)]
        public CharacterData Character = null;

        [YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public MonsterAIData MonsterAI { get; set; } = null;

        [YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public AnimalAIData AnimalAI { get; set; } = null;

        [YamlMember(Order = 6)]
        public TameableData Tameable = null;

        [YamlMember(Order = 7)]
        public GrowupData Growup = null;

        [YamlMember(Order = 8)]
        public ProcreationData Procreation = null;

        [Serializable]
        [CanBeNull]
        internal class CloneData
        {
            public string From { get; set; } = null;
            public string Name { get; set; } = null;
            public bool? RemoveMonsterAI { get; set; } = null; // and replace it with AnimalAI
            //public bool? IsOffspring { get; set; } = null; // todo: remove me

            public float? Scale { get; set; } = null;
            public string[] RemoveEffects { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ComponentsData
        {

            [YamlMember(Order = 1)]
            public ComponentBehavior Character { get; set; } = ComponentBehavior.Patch;

            [YamlMember(Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
            public ComponentBehavior? MonsterAI { get; set; } = null;

            [YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
            public ComponentBehavior? AnimalAI { get; set; } = null;

            [YamlMember(Order = 4)]
            public ComponentBehavior Tameable { get; set; } = ComponentBehavior.Inherit;

            [YamlMember(Order = 5)]
            public ComponentBehavior Growup { get; set; } = ComponentBehavior.Inherit;

            [YamlMember(Order = 6)]
            public ComponentBehavior Procreation { get; set; } = ComponentBehavior.Inherit;

        }

        [Serializable]
        [CanBeNull]
        public class MonsterAIData : BaseAIData
        {
        }

        [Serializable]
        [CanBeNull]
        public class AnimalAIData : BaseAIData
        {
        }

        [Serializable]
        [CanBeNull]
        public class BaseAIData
        {
            [Serializable]
            [CanBeNull]
            public class ConsumItemData
            {
                public string Prefab { get; set; } = null;
                public float FedDurationFactor { get; set; } = 1f; // OTAB feature
            }

            public ConsumItemData[] ConsumeItems { get; set; } = null;
            public float? ConsumeRange { get; set; } = null;
            public float? ConsumeSearchRange { get; set; } = null;
            public float? ConsumeSearchInterval { get; set; } = null;
            public string ConsumeAnimation { get; set; } = null;
            // todo: add "ConsumeAnimationAlt" for food with 0 fedduration factor
            public bool? TamedIdleNearSpawn { get; set; } = null;
            public float? IdleSoundChanceWhenTamed { get; set; } = null; // todo: rename to TamedIdleSoundChance
        }

        [Serializable]
        [CanBeNull]
        public class CharacterData
        {
            public int? MaxLevel { get; set; } = null;

            public string Group { get; set; } = null;
            public string GroupWhenTamed { get; set; } = null;
            public Character.Faction? FactionWhenTamed { get; set; } = null;
            

            public IsEnemyCondition TamedVersusPlayer { get; set; } = IsEnemyCondition.Default;
            public IsEnemyCondition TamedVersusGroup { get; set; } = IsEnemyCondition.Default;
            public IsEnemyCondition TamedVersusFaction { get; set; } = IsEnemyCondition.Default;
            public IsEnemyCondition TamedVersusTamed { get; set; } = IsEnemyCondition.Default;
            public IsEnemyCondition TamedVersusWild { get; set; } = IsEnemyCondition.Default;

            public bool? TameSpawnedOnDeath { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class TameableData
        {
            public bool? FeedingDisabled { get; set; } = null;
            public bool? TamingDisabled { get; set; } = null;
            public float? FedDuration { get; set; } = null;
            public float? TamingTime { get; set; } = null;
            // todo: add option for "m_startsTamed"
            public bool? Commandable { get; set; } = null;
            public string PetCommandText { get; set; } = null;
            public string PetAnswerText { get; set; } = null;
            public bool? ShowPetEffect { get; set; } = null;
            public string RequireGlobalKey { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class GrowupData
        {

            [Serializable]
            [CanBeNull]
            public class GrownData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;
            }

            public float? GrowTime { get; set; } = null;
            public bool? InheritTame { get; set; } = null;
            public bool? RequireFeeding { get; set; } = null;
            public string RequireGlobalKey { get; set; } = null;
            public GrownData[] Grown { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class ProcreationData
        {

            [Serializable]
            [CanBeNull]
            public class OffspringData
            {
                public string Prefab { get; set; } = null;
                public float Weight { get; set; } = 1;

                public bool NeedPartner { get; set; } = true; // true = vanilla
                public string NeedPartnerPrefab { get; set; } = null; // OTAB feature

                public float? LevelUpChance { get; set; } = null; // OTAB feature
                public bool InheritTame { get; set; } = true; // OTAB feature
            }

            public float? UpdateInterval { get; set; } = null;
            public float? TotalCheckRange { get; set; } = null;

            public List<string> Partner { get; set; } = null;
            public float? PartnerCheckRange { get; set; } = null;
            public int? RequiredLovePoints { get; set; } = null;
            // todo: RequiredLovePoints can be 0. love points wont be shown in hover text, 1 is still used for procreation logic

            public float? PregnancyChance { get; set; } = null;
            public float? PregnancyDuration { get; set; } = null;

            public float? SpawnOffset { get; set; } = null;
            public float? SpawnOffsetMax { get; set; } = null;
            public bool? SpawnRandomDirection { get; set; } = null;

            public bool ProcreateWhileSwimming { get; set; } = true; // OTAB feature

            public int? MaxCreatures { get; set; } = null;
            public string[] MaxCreaturesCountPrefabs { get; set; } = null; // OTAB feature

            public OffspringData[] Offspring { get; set; } = null;

        }

    }
}
