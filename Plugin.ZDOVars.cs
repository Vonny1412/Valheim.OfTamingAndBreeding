using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        public static class ZDOVars
        {

            public static class EggBehavior
            {
                public const int Unknown = 0;
                public const int Vanilla = 1;
                public const int OTAB = 2;
            }

            //
            // Creature Tameable
            //

            // used to store current selected fed duration factor of the tamed/untamed creature because it can vary depending on the item it has consumed, based on our Creature.MonsterAIData
            internal static readonly int z_fedDurationFactor = $"{Plugin.ModGuid}.z_fedDurationFactor".GetStableHashCode();

            // todo: short description
            internal static readonly int z_starvingAfter = $"{Plugin.ModGuid}.z_starvingAfter".GetStableHashCode();

            //
            // Creature Procreation
            //

            // counter for offsprings of current pregnancy
            public static readonly int z_siblingsCounter = $"{Plugin.ModGuid}.z_siblingsCounter".GetStableHashCode();

            // used to store if partner is neccessary for running procreation
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int z_needPartner = $"{Plugin.ModGuid}.z_needPartner".GetStableHashCode();

            // used to store current offspring prefab to be used for breeding and max-count check
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int z_offspringPrefab = $"{Plugin.ModGuid}.z_offspringPrefab".GetStableHashCode();

            // used to store current offspring level (we got a level-up feature, remember?)
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int z_offspringLevel = $"{Plugin.ModGuid}.z_offspringLevel".GetStableHashCode();

            // used to store current chosen partner for that parent
            public static readonly int z_partnerPrefab = $"{Plugin.ModGuid}.z_partnerPrefab".GetStableHashCode();

            // used to store a delay timer for re-chosing partner if old one gets invalid (maybe killed, out of range)
            public static readonly int z_partnerNotSeenSince = $"{Plugin.ModGuid}.z_partnerNotSeenSince".GetStableHashCode();

            // used to store if the upcoming offspring spawns tamed or not
            public static readonly int z_offspringTamed = $"{Plugin.ModGuid}.z_offspringTamed".GetStableHashCode();

            //
            // Egg
            //

            // used to store flag if valheim is handling the egg grow update or we (0=undecided, 1=valheim, 2=otab)
            public static readonly int z_EggBehavior = $"{Plugin.ModGuid}.z_EggBehavior".GetStableHashCode();

            // used to store current selected grown prefab for the egg because it can vary based on our egg data
            public static readonly int z_eggGrownPrefab = $"{Plugin.ModGuid}.z_eggGrownPrefab".GetStableHashCode();

            // used to store flag if egg's grown gets automatically tamed because it can vary based on our egg data
            public static readonly int z_eggGrownTamed = $"{Plugin.ModGuid}.z_eggGrownTamed".GetStableHashCode();

            // used to store flag to show hatch effect or not because it can vary based on our egg data
            public static readonly int z_eggShowHatchEffect = $"{Plugin.ModGuid}.z_eggShowHatchEffect".GetStableHashCode();

            //
            // Item
            //

            // used to store a flag on the dropped item if it has been dropped by any player (1=true, 0=false, -1=(default)unknown)
            public static readonly int z_droppedByAnyPlayer = $"{Plugin.ModGuid}.z_droppedByAnyPlayer".GetStableHashCode();

            //
            // CLLC
            //

            // pass traits: parent [ -> egg(N) ] [ -> offspring ] -> adult
            public static readonly int z_CLLC_Infusion = $"{Plugin.ModGuid}.z_CLLC_Infusion".GetStableHashCode();
            public static readonly int z_CLLC_Effect = $"{Plugin.ModGuid}.z_CLLC_Effect".GetStableHashCode();

        }
    }
}
