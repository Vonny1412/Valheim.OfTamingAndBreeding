using OfTamingAndBreeding.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{
    internal partial class CharacterAPI : API.Character
    {

        private static readonly ConditionalWeakTable<Character, CharacterAPI> instances
            = new ConditionalWeakTable<Character, CharacterAPI>();

        public static CharacterAPI GetOrCreate(Character __instance)
        {
            return instances.GetValue(__instance, inst =>
            {
                Lifecycle.CleanupMarks.Mark(inst.GetComponent<ZNetView>());
                return new CharacterAPI(inst);
            });
        }

        public static bool TryGet(Character __instance, out CharacterAPI api)
            => instances.TryGetValue(__instance, out api);
        public static void Remove(Character __instance)
            => instances.Remove(__instance);

        public CharacterAPI(Character __instance) : base(__instance)
        {
        }

    }
}
