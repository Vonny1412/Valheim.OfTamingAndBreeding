using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Internals
{
    /*
    internal class MonsterAIAPI : Internals.API.MonsterAI
    {
        private static readonly ConditionalWeakTable<MonsterAI, MonsterAIAPI> instances
            = new ConditionalWeakTable<MonsterAI, MonsterAIAPI>();

        public static MonsterAIAPI GetOrCreate(MonsterAI __instance)
        {
            return instances.GetValue(__instance, inst =>
            {
                Lifecycle.CleanupMarks.Mark(inst.GetComponent<ZNetView>());
                return new MonsterAIAPI(inst);
            });
        }

        public static bool TryGet(MonsterAI __instance, out MonsterAIAPI api)
            => instances.TryGetValue(__instance, out api);
        public static void Remove(MonsterAI __instance)
            => instances.Remove(__instance);

        public MonsterAIAPI(MonsterAI __instance) : base(__instance)
        {
            // not used yet
        }

    }
    */
}
