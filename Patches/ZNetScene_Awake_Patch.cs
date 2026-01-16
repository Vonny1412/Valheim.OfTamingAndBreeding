
using HarmonyLib;
using OfTamingAndBreeding.Net;
using System.IO;

using OfTamingAndBreeding.Data;

namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    static class ZNetScene_Awake_Patch
    {

        [HarmonyPriority(Priority.Last)]
        static void Postfix(ZNetScene __instance)
        {
            //var zn = ZNet.instance;
            //Plugin.Log.LogWarning($"[OTAB] ZNetScene.Awake POSTFIX | ZNet={(zn ? "yes" : "no")} IsServer={(zn?.IsServer() ?? false)} IsDedicated={(zn?.IsDedicated() ?? false)} World='{zn?.GetWorldName()}'");
            //Plugin.Log.LogWarning($"[OTAB] ServerDataDir='{Plugin.ServerDataDir}' CacheDir='{Plugin.CacheDir}'");

            RPCContext.Init(); // for both client and server

            if (ZNet.instance && ZNet.instance.IsServer())
            {
                DataSaver saver;

                var originalDataPath = Path.Combine(Plugin.ServerDataDir, Plugin.OriginalDataDirectory);
                if (!Directory.Exists(originalDataPath))
                {

                    saver = new DataSaver();

                    saver.AddObject("Boar", DataSaver.ObjectType.Parent, true);
                    saver.AddObject("Wolf", DataSaver.ObjectType.Parent, true);
                    saver.AddObject("Lox", DataSaver.ObjectType.Parent, true);
                    saver.AddObject("Hen", DataSaver.ObjectType.Parent, true);
                    saver.AddObject("Asksvin", DataSaver.ObjectType.Parent, true);

                    saver.AddObject("Bjorn", DataSaver.ObjectType.Parent, false);
                    saver.AddObject("Deer", DataSaver.ObjectType.Parent, false);
                    saver.AddObject("Neck", DataSaver.ObjectType.Parent, false);

                    saver.AddObject("DragonEgg", DataSaver.ObjectType.Item, true); // not handled yet, maybe implement Items later?

                    saver.WriteFiles(originalDataPath);
                }

                RPCContext.InitServer();
            }
            else
            {
                RPCContext.InitClient();
            }
        }
    }

}
