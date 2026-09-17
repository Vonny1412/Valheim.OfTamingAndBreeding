using JetBrains.Annotations;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using YamlDotNet.Serialization;

namespace OfTamingAndBreeding.Data.Models
{
    [Serializable]
    internal class CreatureFile : DataBase<CreatureFile>
    {

        public const string DirectoryName = "Creatures";

        [YamlMember(Order = 1)]
        public ComponentsData Components = new ComponentsData();

        [YamlMember(Order = 2)]
        public CharacterAIData Character = null;

        [YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public MonsterAIData MonsterAI { get; set; } = null;

        [YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public AnimalAIData AnimalAI { get; set; } = null;

        [YamlMember(Order = 5)]
        public TameableData Tameable = null;

        [YamlMember(Order = 6)]
        public ProcreationData Procreation = null;

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
            public ComponentBehavior Procreation { get; set; } = ComponentBehavior.Inherit;
        }

        [Serializable]
        [CanBeNull]
        public class CharacterAIData
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
            public BaseAIConsumItemData[] ConsumeItems { get; set; } = null;
            public float? ConsumeRange { get; set; } = null;
            public float? ConsumeSearchRange { get; set; } = null;
            public float? ConsumeSearchInterval { get; set; } = null;
            public string ConsumeAnimation { get; set; } = null;
            // todo: add "ConsumeAnimationAlt" for food with 0 fedduration factor
            public bool TamedStayNearSpawn { get; set; } = false; // otab feature
            public float? IdleSoundChanceWhenTamed { get; set; } = null;
        }

        [Serializable]
        [CanBeNull]
        public class BaseAIConsumItemData
        {
            public string Prefab { get; set; } = null;
            public float FedDurationFactor { get; set; } = 1f; // OTAB feature
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
                public bool SpawnTamed { get; set; } = true; // OTAB feature
            }

            public float? UpdateInterval { get; set; } = null;
            public float? TotalCheckRange { get; set; } = null;

            public PartnerData[] Partner { get; set; } = null;
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
