using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OfTamingAndBreeding.Internals
{

    internal class TameableAPI : API.Tameable
    {

        private static readonly ConditionalWeakTable<Tameable, TameableAPI> instances
            = new ConditionalWeakTable<Tameable, TameableAPI>();
        public static TameableAPI GetOrCreate(Tameable __instance)
            => instances.GetValue(__instance, (Tameable inst) => new TameableAPI(inst));
        public static bool TryGetAPI(Tameable __instance, out TameableAPI api)
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

        public string GetFeedingHoverText()
        {
            ZDO zDO = m_nview.GetZDO();
            if (zDO == null)
            {
                return "";
            }

            var text = new List<string>();

            var L = Localization.instance;

            if (Plugin.Configs.HoverShowFed.Value)
            {
                // currently we do not handly modified fed durations
                // maybe we gonna implement it later
                var duration = m_fedDuration;

                var lastFedTimeLong = zDO.GetLong(ZDOVars.s_tameLastFeeding, 0L);
                DateTime dateTime = new DateTime(lastFedTimeLong);
                double secLeft = duration - (ZNet.instance.GetTime() - dateTime).TotalSeconds;

                if (Plugin.Configs.HoverShowFedTimer.Value && (secLeft > 0 || Plugin.Configs.HoverShowFedTimerStarving.Value))
                {
                    if (lastFedTimeLong == 0)
                    {
                        secLeft = -m_monsterAI.GetTimeSinceSpawned().TotalSeconds;
                    }
                    text.Add(Helpers.StringHelper.FormatRelativeTime(
                        secLeft,
                        labelPositive: L.Localize("$tmt_hover_fed"),
                        labelNegative: L.Localize("$tmt_hover_starving"),
                        labelAltPositive: L.Localize("$tmt_hover_fed_alt"),
                        labelAltNegative: L.Localize("$tmt_hover_starving_alt"),
                        colorPositive: Plugin.Configs.HoverColorGood.Value,
                        colorNegative: Plugin.Configs.HoverColorBad.Value
                    ));
                    /*
                    if (Plugin.Configs.HoverShowDurations.Value)
                    {
                        text.Add(string.Format(L.Localize("$tmt_hover_duration"), Plugin.Configs.HoverColorNormal.Value, duration));
                    }
                    */
                }
            }

            return string.Join("\n", text.Where((string line) => line.Trim() != ""));
        }

    }

}
