using BepInEx;

namespace OfTamingAndBreeding
{
    //[BepInDependency("InternalsAPI.Valheim")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed partial class Plugin : BaseUnityPlugin
	{
		// once-generated file – use it to declare dependencies
	}
}
