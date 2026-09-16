using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class AlwaysActivePatches : Core.PatchGroup<AlwaysActivePatches>
    {
        internal static new void Install() => Core.PatchGroup<AlwaysActivePatches>.Install();
        internal static new void Uninstall() => Core.PatchGroup<AlwaysActivePatches>.Uninstall();

    }
}
