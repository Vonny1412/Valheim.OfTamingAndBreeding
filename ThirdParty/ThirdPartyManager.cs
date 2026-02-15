using BepInEx;
using BepInEx.Bootstrap;
using OfTamingAndBreeding.ThirdParty.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Switch;

namespace OfTamingAndBreeding.ThirdParty
{

    internal interface IThirdPartyPluginRegistrator
    {
        string PluginGUID { get; }
        void OnRegistered(string guid, Assembly asm);
    }

    internal abstract class ThirdPartyPluginRegistrator : IThirdPartyPluginRegistrator
    {
        public abstract string PluginGUID { get; }

        public abstract void OnRegistered(string guid, Assembly asm);

    }

    internal static class ThirdPartyManager
    {

        public static bool TryGetPluginMetadata(string GUID, out BepInPlugin meta)
        {
            if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
            {
                meta = info.Metadata;
                return true;
            }
            meta = null;
            return false;
        }

        private static bool TryGetPluginAssembly(string GUID, out Assembly asm)
        {
            if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
            {
                asm = info.Instance.GetType().Assembly;
                return asm != null;
            }
            asm = null;
            return false;
        }

        public static void RegisterBridges()
        {
            var regs = new List<IThirdPartyPluginRegistrator>()
            {
                new WackyDBBridge.Registrator(),
                new CllCBridge.Registrator(),
            };
            foreach (IThirdPartyPluginRegistrator reg in regs)
            {
                string guid = reg.PluginGUID;
                Action<string, Assembly> cb = reg.OnRegistered;
                if (TryGetPluginAssembly(guid, out Assembly asm))
                {
                    cb(guid, asm);
                }
            }
        }

    }
}
