using HarmonyLib;
using OfTamingAndBreeding.Data;
using OfTamingAndBreeding.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ZNet), "Start")]
    static class ZNet_Start_Patch
    {
        static void Postfix()
        {
            var zn = ZNet.instance;
            if (zn.IsServer()) RPCContext.InitServerSession();
            else RPCContext.InitClientSession();



            /*

            if (zn.IsServer())
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

            }
            */
        }
    }
}
