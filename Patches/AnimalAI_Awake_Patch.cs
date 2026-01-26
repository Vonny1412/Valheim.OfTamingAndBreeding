using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Patches
{
    [HarmonyPatch(typeof(AnimalAI), "Awake")]
    static class AnimalAI_Awake_Patch
    {
        static void Postfix(AnimalAI __instance)
        {
            // we need this because we wanna pass settings from prefab to object
            var prefabName = Utils.GetPrefabName(__instance.gameObject.name);
            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab) return; // should not happen but whatever
            var prefabAnimalAI = prefab.GetComponent<AnimalAI>();
            if (!prefabAnimalAI) return; // should not happen but whatever

            // check if animalAIAPI of prefab is stored
            if (!Internals.AnimalAIAPI.TryGetAPI(prefabAnimalAI, out Internals.AnimalAIAPI prefabAnimalAIAPI))
            {
                // the prefabAnimalAI is not stored -> not an animal we need to handle
                return;
            }

            var animalAIAPI = Internals.AnimalAIAPI.GetOrCreate(__instance); // register api for the animalAI of the object
            // take and set values: prefabAnimalAIAPI -> animalAIAPI
            animalAIAPI.m_consumeRange = prefabAnimalAIAPI.m_consumeRange;
            animalAIAPI.m_consumeSearchRange = prefabAnimalAIAPI.m_consumeSearchRange;
            animalAIAPI.m_consumeSearchInterval = prefabAnimalAIAPI.m_consumeSearchInterval;
            animalAIAPI.m_onConsumedItem = prefabAnimalAIAPI.m_onConsumedItem;
            animalAIAPI.m_consumeItems = new List<ItemDrop>(prefabAnimalAIAPI.m_consumeItems);
        }
    }

    /** original method
    protected override void Awake()
    {
        base.Awake();
        m_updateTargetTimer = Random.Range(0f, 2f);
    }
    **/

}
