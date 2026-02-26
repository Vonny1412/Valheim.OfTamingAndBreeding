using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches.Base
{
    internal class PatchGroup<T> where T : class
    {
        private static Harmony _harmony;

        private static string HarmonyId => $"OTAB.{typeof(T).Name}";

        protected static void Install()
        {
            if (_harmony != null)
            {
                return;
            }

            _harmony = new Harmony(HarmonyId);
            try
            {
                _harmony.PatchAll(typeof(T));
            }
            catch
            {
                try
                {
                    _harmony.UnpatchAll(HarmonyId);
                }
                catch
                {
                }
                _harmony = null;
                throw;
            }
        }

        protected static void Uninstall()
        {
            if (_harmony == null)
            {
                return;
            }
            
            _harmony.UnpatchAll(HarmonyId);
            _harmony = null;
        }
    }
}
