using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{

    internal class GrowupAPI : API.Growup
    {
        private static readonly ConditionalWeakTable<Growup, GrowupAPI> instances
            = new ConditionalWeakTable<Growup, GrowupAPI>();
        public static GrowupAPI GetOrCreate(Growup __instance)
            => instances.GetValue(__instance, (Growup inst) => new GrowupAPI(inst));
        public static bool TryGet(Growup __instance, out GrowupAPI api)
            => instances.TryGetValue(__instance, out api);

        public GrowupAPI(Growup __instance) : base(__instance)
        {
        }

        public bool GrowUpdate_Prefix(ZDO zdo)
        {
            // m_baseAI is set on awake. shouldnt be null. if its null, i dont care
            if (!(m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)m_growTime))
            {
                return false; // like default
            }

            // vanilla block start
            Character myCharacter = GetComponent<Character>();
            GameObject spawned = UnityEngine.Object.Instantiate(GetPrefab(), transform.position, transform.rotation);
            Character spawnedCharacter = spawned.GetComponent<Character>();
            if ((bool)myCharacter && (bool)spawnedCharacter)
            {
                if (m_inheritTame)
                {
                    spawnedCharacter.SetTamed(myCharacter.IsTamed());
                }

                spawnedCharacter.SetLevel(myCharacter.GetLevel());
            }
            // vanilla block end

            ThirdParty.Mods.CllCBridge.PassTraits(zdo, spawned);

            m_nview.Destroy();
            return false;
        }

    }

}
