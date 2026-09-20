using System;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        public static class ZDOVars
        {

            internal static readonly int z_fedDurationFactor = $"{Plugin.ModGuid}.z_fedDurationFactor".GetStableHashCode();
            public static readonly int z_partnerPrefab = $"{Plugin.ModGuid}.z_partnerPrefab".GetStableHashCode();
            public static readonly int z_confined = $"{Plugin.ModGuid}.z_confined".GetStableHashCode(); // Anti-Exploit-System

            public static readonly int z_droppedByAnyPlayer = $"{Plugin.ModGuid}.z_droppedByAnyPlayer".GetStableHashCode();

            public static readonly int z_CLLC_Infusion = $"{Plugin.ModGuid}.z_CLLC_Infusion".GetStableHashCode();
            public static readonly int z_CLLC_Effect = $"{Plugin.ModGuid}.z_CLLC_Effect".GetStableHashCode();

        }
    }
}
