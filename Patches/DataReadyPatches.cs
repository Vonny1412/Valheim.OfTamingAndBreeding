using HarmonyLib;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch]
    internal partial class DataReadyPatches : Core.PatchGroup<DataReadyPatches>
    {
        internal static new void Install() => Core.PatchGroup<DataReadyPatches>.Install();
        internal static new void Uninstall() => Core.PatchGroup<DataReadyPatches>.Uninstall();

    }
}
