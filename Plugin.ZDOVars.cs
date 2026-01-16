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
                public const int Matrjoschka = 2;
            }

            //
            // Creature Tameable
            //

            // used to store current selected fed duration of the tamed/untamed creature because it can vary depending on the item it has consumed, based on our Creature.MonsterAIData
            internal static readonly int s_fedDuration = $"{Plugin.ModGuid}.s_fedDuration".GetStableHashCode();

            // used to store the last consumed item prefab name
            // because it affects selected offspring
            //internal static readonly int s_lastConsumedItem = $"{Plugin.ModGuid}.s_lastConsumedItem".GetStableHashCode();

            //
            // Creature Procreation
            //

            // counter for offsprings of current pregnancy
            public static readonly int s_offspringCounter = $"{Plugin.ModGuid}.s_offspringCounter".GetStableHashCode();

            // used to store current max offsprings count
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int s_maxCreatures = $"{Plugin.ModGuid}.s_maxCreatures".GetStableHashCode();

            // used to store if partner is neccessary for running procreation
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int s_needPartner = $"{Plugin.ModGuid}.s_needPartner".GetStableHashCode();

            // used to store current offspring prefab to be used for breeding and max-count check
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int s_offspringPrefab = $"{Plugin.ModGuid}.s_offspringPrefab".GetStableHashCode();

            // used to store current offspring level (we got a level-up feature, remember?)
            // because it can vary depending on randomly chosen offspring from data
            public static readonly int s_offspringLevel = $"{Plugin.ModGuid}.s_offspringLevel".GetStableHashCode();

            // used to trigger new choosing of partner for procreation
            public static readonly int s_doResetPartner = $"{Plugin.ModGuid}.s_doResetPartner".GetStableHashCode();

            // used to trigger new choosing of offspring based on chosen partner
            public static readonly int s_doResetOffspring = $"{Plugin.ModGuid}.s_doResetOffspring".GetStableHashCode();

            // used to store current chosen partner for that parent
            public static readonly int s_partnerPrefab = $"{Plugin.ModGuid}.s_partnerPrefab".GetStableHashCode();

            // used to store a delay timer for re-chosing partner if old one gets invalid (maybe killed, out of range)
            public static readonly int s_partnerLastSeen = $"{Plugin.ModGuid}.s_partnerLastSeen".GetStableHashCode();

            //
            // Egg
            //

            // used to store flag if valheim is handling the egg grow update or we (0=undecided, 1=valheim, 2=we)
            public static readonly int s_EggBehavior = $"{Plugin.ModGuid}.s_EggBehavior".GetStableHashCode();

            // used to store current selected grown prefab for the egg because it can vary based on our egg data
            public static readonly int s_eggGrownPrefab = $"{Plugin.ModGuid}.s_eggGrownPrefab".GetStableHashCode();

            // used to store flag if egg's grown gets automatically tamed because it can vary based on our egg data
            public static readonly int s_eggGrownTamed = $"{Plugin.ModGuid}.s_eggGrownTamed".GetStableHashCode();

            // used to store flag to show hatch effect or not because it can vary based on our egg data
            public static readonly int s_eggShowHatchEffect = $"{Plugin.ModGuid}.s_eggShowHatchEffect".GetStableHashCode();

            //
            // Item
            //

            // used to to a flag on the dropped item if it has been dropped by any player (1=true, 0=false, -1=(default)unknown)
            public static readonly int s_droppedByAnyPlayer = $"{Plugin.ModGuid}.s_droppedByAnyPlayer".GetStableHashCode();

        }
    }
}
