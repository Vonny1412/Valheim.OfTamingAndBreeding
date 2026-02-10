using HarmonyLib;
using OfTamingAndBreeding.Data.Models.SubData;
using OfTamingAndBreeding.Internals;
using OfTamingAndBreeding.Internals.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace OfTamingAndBreeding.Patches
{

    [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
    static class BaseAI_IsEnemy_Patch
    {

        [HarmonyPriority(Priority.Last)]
        static bool Prefix(BaseAI __instance, Character a, Character b, ref bool __result)
        {
            Contexts.IsEnemyContext.Depth++;
            bool isOuterMost = (Contexts.IsEnemyContext.Depth == 1);
            if (!isOuterMost)
            {
                return true;
            }

            if (!Helpers.ZNetHelper.TryGetZDO(a, out ZDO zdo1, out ZNetView nview1))
            {
                return true; // i dont care
            }
            if (!Helpers.ZNetHelper.TryGetZDO(b, out ZDO zdo2, out ZNetView nview2))
            {
                return true; // i dont care
            }

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();
            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // both are wild => let valheim decide
            if (isTamed1 == isTamed2 && !isTamed1)
            {
                return true;
            }

            var name1 = Utils.GetPrefabName(a.gameObject.name);
            var name2 = Utils.GetPrefabName(b.gameObject.name);

            // handle stick-to-faction
            var stickToFaction1 = Contexts.DataContext.GetSticksToFaction(name1);
            var stickToFaction2 = Contexts.DataContext.GetSticksToFaction(name2);
            if (isTamed1 && stickToFaction1 || isTamed2 && stickToFaction2)
            {
                if (faction1 == faction2)
                {
                    __result = false;
                    return false;
                }
                else if (faction1 == Character.Faction.Undead && faction2 == Character.Faction.Demon)
                {
                    __result = false;
                    return false;
                }
                else if (faction1 == Character.Faction.Demon && faction2 == Character.Faction.Undead)
                {
                    __result = false;
                    return false;
                }
            }

            var zTime = ZNet.instance.GetTime();
            var tameable1 = a.GetComponent<Tameable>();
            var tameable2 = b.GetComponent<Tameable>();
            var isStarving1 = Contexts.DataContext.TryGetStarvingDelay(name1, out float starvingDelay1) && tameable1 && TameableAPI.GetFedTimeLeft(tameable1, zdo1, zTime) < -starvingDelay1;
            var isStarving2 = Contexts.DataContext.TryGetStarvingDelay(name2, out float starvingDelay2) && tameable2 && TameableAPI.GetFedTimeLeft(tameable2, zdo2, zTime) < -starvingDelay2;

            if (isTamed1 && isTamed2)
            {
                var canAttack = false;

                switch (Contexts.DataContext.GetCanAttackTames(name1))
                {
                    case IsEnemyCondition.Never:
                        canAttack = false;
                        break;
                    case IsEnemyCondition.Always:
                        canAttack = true;
                        break;
                    case IsEnemyCondition.WhenStarving:
                        canAttack = isStarving1;
                        break;
                }
                
                if (!canAttack)
                {
                    switch (Contexts.DataContext.GetCanBeAttackedByTames(name2))
                    {
                        case IsEnemyCondition.Never:
                            canAttack = false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack = true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack = isStarving2;
                            break;
                    }
                }

                if (canAttack)
                {
                    Contexts.IsEnemyContext.Active = true;
                    Contexts.IsEnemyContext.TargetInstance = b;
                }

            }
            else
            {
                if (isTamed1 && faction2 == Character.Faction.Players)
                {
                    var canAttack = false;
                    switch (Contexts.DataContext.GetCanAttackPlayer(name1))
                    {
                        case IsEnemyCondition.Never:
                            canAttack = false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack = true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack = isStarving1;
                            break;
                    }
                    if (canAttack)
                    {
                        __result = true;
                        return false;
                    }
                }
            }

            return true; // let valheim decide
        }

        static void Finalizer(Exception __exception)
        {
            Contexts.IsEnemyContext.Depth--;
            if (Contexts.IsEnemyContext.Depth <= 0)
            {
                Contexts.IsEnemyContext.Depth = 0;
                Contexts.IsEnemyContext.Active = false;
                Contexts.IsEnemyContext.TargetInstance = null;
            }
        }

    }

    /** original method
    public static bool IsEnemy(Character a, Character b)
    {
        if (a == b)
        {
            return false;
        }

        if (!a || !b)
        {
            return false;
        }

        string group = a.GetGroup();
        if (group.Length > 0 && group == b.GetGroup())
        {
            return false;
        }

        Character.Faction faction = a.GetFaction();
        Character.Faction faction2 = b.GetFaction();
        bool flag = a.IsTamed();
        bool flag2 = b.IsTamed();
        bool flag3 = (bool)a.GetBaseAI() && a.GetBaseAI().IsAggravated();
        bool flag4 = (bool)b.GetBaseAI() && b.GetBaseAI().IsAggravated();
        if (flag || flag2)
        {
            if ((flag && flag2) || (flag && faction2 == Character.Faction.Players) || (flag2 && faction == Character.Faction.Players) || (flag && faction2 == Character.Faction.Dverger && !flag4) || (flag2 && faction == Character.Faction.Dverger && !flag3))
            {
                return false;
            }

            return true;
        }

        if ((flag3 || flag4) && ((flag3 && faction2 == Character.Faction.Players) || (flag4 && faction == Character.Faction.Players)))
        {
            return true;
        }

        if (faction == faction2)
        {
            return false;
        }

        switch (faction)
        {
            case Character.Faction.AnimalsVeg:
            case Character.Faction.PlayerSpawned:
                return true;
            case Character.Faction.Players:
                return faction2 != Character.Faction.Dverger;
            case Character.Faction.ForestMonsters:
                if (faction2 != Character.Faction.AnimalsVeg)
                {
                    return faction2 != Character.Faction.Boss;
                }

                return false;
            case Character.Faction.Undead:
                if (faction2 != Character.Faction.Demon)
                {
                    return faction2 != Character.Faction.Boss;
                }

                return false;
            case Character.Faction.Demon:
                if (faction2 != Character.Faction.Undead)
                {
                    return faction2 != Character.Faction.Boss;
                }

                return false;
            case Character.Faction.MountainMonsters:
                return faction2 != Character.Faction.Boss;
            case Character.Faction.SeaMonsters:
                return faction2 != Character.Faction.Boss;
            case Character.Faction.PlainsMonsters:
                return faction2 != Character.Faction.Boss;
            case Character.Faction.MistlandsMonsters:
                if (faction2 != Character.Faction.AnimalsVeg)
                {
                    return faction2 != Character.Faction.Boss;
                }

                return false;
            case Character.Faction.Dverger:
                if (faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss)
                {
                    return faction2 != Character.Faction.Players;
                }

                return false;
            case Character.Faction.Boss:
                if (faction2 != 0)
                {
                    return faction2 == Character.Faction.PlayerSpawned;
                }

                return true;
            case Character.Faction.TrainingDummy:
                return faction2 == Character.Faction.Players;
            default:
                return false;
        }
    }
    **/

}
