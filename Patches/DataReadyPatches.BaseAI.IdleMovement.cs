using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Extensions;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "IdleMovement")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BaseAI_IdleMovement_Prefix(BaseAI __instance, float dt)
        {
            if (__instance.TryGetComponent<ConsumeAnimationClipOverlay>(out var clip) && clip.IsPlaying())
            {
                return false;
            }

            if (__instance is AnimalAI animalAI)
            {
                if (animalAI.TryGetComponent<ExtendedAnimaAI>(out var exAnimaAI))
                {
                    // used for animals that can consume food or follow the player
                    // that stuff comes always before idle movement
                    if (exAnimaAI.IdleMovement(dt))
                    {
                        return false;
                    }
                }
            }

            /* // original:
            protected void IdleMovement(float dt)
            {
                Vector3 centerPoint = ((m_character.IsTamed() || HuntPlayer()) ? base.transform.position : m_spawnPoint);
                if (GetPatrolPoint(out var point))
                {
                    centerPoint = point;
                }

                RandomMovement(dt, centerPoint, snapToGround: true);
            }
            */

            var character = __instance.GetComponent<Character>();
            if (!character || !character.IsTamed() || __instance.HuntPlayer()) return true;

            if (__instance.TryGetComponent<OTABCreature>(out var creature))
            {
                if (creature.m_tamedStayNearSpawn == true)
                {
                    if (__instance.GetPatrolPoint(out _))
                    {
                        // is commandable and currently not following
                        // => bound to partol point
                        return true;
                    }

                    __instance.RandomMovement(dt, __instance.GetSpawnPoint(), snapToGround: true);
                    return false;
                }
            }

            return true;
        }

    }
}
