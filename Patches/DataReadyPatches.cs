using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Base.PatchGroup<DataReadyPatches>
    {
        internal static new void Install() => Base.PatchGroup<DataReadyPatches>.Install();
        internal static new void Uninstall() => Base.PatchGroup<DataReadyPatches>.Uninstall();
    }
}
