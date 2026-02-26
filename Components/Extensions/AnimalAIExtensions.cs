using OfTamingAndBreeding.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Components.Extensions
{
    internal static class AnimalAIExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAlerted(this AnimalAI that, bool alert)
            => ValheimAPI.AnimalAI.__IAPI_SetAlerted_Invoker1.Invoke(that, alert);

        public static bool IdleMovement_PatchPrefix(this AnimalAI animalAI, float dt)
        {
            var m_nview = animalAI.GetZNetView();
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                // we dont need to check for owner
                // because IdleMovement only gets called when it is the owner
                // but whatever
                return true; // i dont care
            }

            if (animalAI.TryGetComponent<OTAB_CustomAnimalAI>(out var customAnimalAI))
            {
                Humanoid humanoid = animalAI.GetComponent<Character>() as Humanoid;
                if (customAnimalAI.UpdateConsumeItem(humanoid, dt)) return false;
                if (customAnimalAI.UpdateFollowTarget(dt)) return false;
            }

            return true;
        }

    }
}
