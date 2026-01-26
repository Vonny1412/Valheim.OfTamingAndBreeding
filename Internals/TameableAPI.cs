using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{

    internal partial class TameableAPI : API.Tameable
    {

        private static readonly ConditionalWeakTable<Tameable, TameableAPI> instances
            = new ConditionalWeakTable<Tameable, TameableAPI>();
        public static TameableAPI GetOrCreate(Tameable __instance)
            => instances.GetValue(__instance, (Tameable inst) => new TameableAPI(inst));
        public static bool TryGet(Tameable __instance, out TameableAPI api)
            => instances.TryGetValue(__instance, out api);

        //public Data.Models.Creature creatureData;
        public float lastCommandTime = 0;

        public TameableAPI(Tameable __instance) : base(__instance)
        {
            //var prefabName = Utils.GetPrefabName(__instance.name);
            //this.creatureData = Data.Models.Creature.Get(prefabName);
        }

        #region tameable animals

        public AnimalAIAPI animalAIAPI;

        public bool IsAnimal()
        {
            return animalAIAPI != null;
        }

        public void TamingAnimalUpdate()
        {
            if (m_nview.IsValid() && m_nview.IsOwner() && !IsTamed() && !IsHungry() && !animalAIAPI.IsAlerted())
            {
                DecreaseRemainingTime(3f);
                if (GetRemainingTime() <= 0f)
                {
                    Tame();
                }
                else
                {
                    m_sootheEffect.Create(transform.position, transform.rotation);
                }
            }
        }

        public void TameAnimal()
        {
            Game.instance.IncrementPlayerStat(PlayerStatType.CreatureTamed);
            if (m_nview.IsValid() && m_nview.IsOwner() && (bool)m_character && !IsTamed())
            {
                animalAIAPI.MakeTame();
                m_tamedEffect.Create(transform.position, transform.rotation);
                Player closestPlayer = Player.GetClosestPlayer(transform.position, 30f);
                if ((bool)closestPlayer)
                {
                    closestPlayer.Message(MessageHud.MessageType.Center, m_character.m_name + " $hud_tamedone");
                }
            }
        }

        #endregion

    }

}
