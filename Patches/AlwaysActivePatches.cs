using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class AlwaysActivePatches : Base.PatchGroup<AlwaysActivePatches>
    {
        internal static new void Install() => Base.PatchGroup<AlwaysActivePatches>.Install();
        internal static new void Uninstall() => Base.PatchGroup<AlwaysActivePatches>.Uninstall();
    }
}
