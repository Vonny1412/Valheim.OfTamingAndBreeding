using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(ZNet), "OnDestroy")]
    static class ZNet_OnDestroy_Patch
    {
        static void Prefix()
        {
            Data.DataLoader.ResetData();
        }
    }

    /** original method
    private void OnDestroy()
    {
        ZLog.Log("ZNet OnDestroy");
        if (m_instance == this)
        {
            m_instance = null;
        }
    }
    **/

}
