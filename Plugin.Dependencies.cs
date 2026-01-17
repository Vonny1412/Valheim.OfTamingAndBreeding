using BepInEx;
using Jotunn.Utils;

namespace OfTamingAndBreeding
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]

    // Seasons mod can alter pregnancy durations on specific seasons
    // it seems that the Seasons mod is using prefix-patch on Procreation.Procreate with first priority
    // anyway... let it load before OTAB
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]

    // ensure client has this mod with correct version
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]

    public sealed partial class Plugin : BaseUnityPlugin
	{
		// once-generated file – use it to declare dependencies
	}

}
