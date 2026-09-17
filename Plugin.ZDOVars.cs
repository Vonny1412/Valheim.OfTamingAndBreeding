using System;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        public static class ZDOVars
        {

            //
            // Creature Tameable
            //

            // used to store current selected fed duration factor of the tamed/untamed creature because it can vary depending on the item it has consumed, based on our Creature.MonsterAIData
            internal static readonly int z_fedDurationFactor = $"{Plugin.ModGuid}.z_fedDurationFactor".GetStableHashCode();

            //
            // Creature Procreation
            //

            // used to store the partner prefab of the current pregnancy
            public static readonly int z_partnerPrefab = $"{Plugin.ModGuid}.z_partnerPrefab".GetStableHashCode();

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
