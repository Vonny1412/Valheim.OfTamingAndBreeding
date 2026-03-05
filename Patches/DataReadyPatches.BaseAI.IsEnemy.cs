using HarmonyLib;
using OfTamingAndBreeding.Components;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models.SubData;
using System;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static bool BaseAI_IsEnemy_Prefix(BaseAI __instance, Character a, Character b, ref bool __result)
        {
            StaticContext.IsEnemyContext.Depth++;
            bool isOuterMost = (StaticContext.IsEnemyContext.Depth == 1);
            if (!isOuterMost)
            {
                return true;
            }

            // both are players => let valheim decide
            if (a.IsPlayer() && b.IsPlayer())
            {
                return true;
            }

            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // both are wild => let valheim decide
            if (isTamed1 == isTamed2 && !isTamed1)
            {
                return true;
            }

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();
            string group1 = a.m_group;
            string group2 = b.m_group;

            var custom1 = a.GetComponent<OTABCreature>();
            var custom2 = b.GetComponent<OTABCreature>();

            var tameableTrait1 = a.GetComponent<TameableTrait>();
            var tameableTrait2 = b.GetComponent<TameableTrait>();

            var isHungry1 = tameableTrait1 && tameableTrait1.IsHungry();
            var isHungry2 = tameableTrait2 && tameableTrait2.IsHungry();
            var isStarving1 = tameableTrait1 && tameableTrait1.IsStarving();
            var isStarving2 = tameableTrait2 && tameableTrait2.IsStarving();

            var attackTamed1 = custom1?.m_tamedCanAttackTamed ?? IsEnemyCondition.Never;
            var attackByTamed2 = custom2?.m_tamedCanBeAttackedByTamed ?? IsEnemyCondition.Never;
            var attackPlayer1 = custom1?.m_tamedCanAttackPlayer ?? IsEnemyCondition.Never;
            var attackByPlayer2 = custom2?.m_tamedCanBeAttackedByPlayer ?? IsEnemyCondition.Never;
            var attackGroup1 = custom1?.m_tamedCanAttackPlayer ?? IsEnemyCondition.Never;
            var attackByGroup2 = custom2?.m_tamedCanBeAttackedByGroup ?? IsEnemyCondition.Never;
            var attackFaction1 = custom1?.m_tamedCanAttackFaction ?? IsEnemyCondition.Always;
            var attackByFaction2 = custom2?.m_tamedCanBeAttackedByFaction ?? IsEnemyCondition.Always;

            var isSameTamed = isTamed1 && isTamed2;
            var isSameGroup = group1 == group2;
            var isSameFaction = faction1 == faction2 || ((faction1 == Character.Faction.Undead || faction1 == Character.Faction.Demon) && (faction2 == Character.Faction.Undead || faction2 == Character.Faction.Demon));

            var canAttack = false;

            if (b.IsPlayer())
            {
                if (isTamed1)
                {
                    // aggression tamed vs player
                    switch (attackPlayer1)
                    {
                        case IsEnemyCondition.Never:
                            canAttack |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttack |= isHungry1 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttack |= isHungry1 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack |= isStarving1 == true;
                            break;
                    }
                }
            }
            else if (a.IsPlayer())
            {
                if (isTamed2)
                {
                    // aggression player vs tamed
                    switch (attackByPlayer2)
                    {
                        case IsEnemyCondition.Never:
                            canAttack |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttack |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttack |= isHungry2 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttack |= isHungry2 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttack |= isStarving2 == true;
                            break;
                    }
                }
            }
            else
            {
                var canAttackTamed = false;
                var canAttackGroup = false;
                var canAttackFaction = false;

                if (isSameTamed)
                {
                    // aggression tamed vs tamed

                    switch (attackTamed1)
                    {
                        case IsEnemyCondition.Never:
                            canAttackTamed |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackTamed |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackTamed |= isHungry1 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackTamed |= isHungry1 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackTamed |= isStarving1 == true;
                            break;
                    }

                    switch (attackByTamed2)
                    {
                        case IsEnemyCondition.Never:
                            canAttackTamed |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackTamed |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackTamed |= isHungry2 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackTamed |= isHungry2 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackTamed |= isStarving2 == true;
                            break;
                    }

                }

                if (isSameGroup)
                {
                    // aggression tamed vs group

                    switch (attackGroup1)
                    {
                        case IsEnemyCondition.Never:
                            canAttackGroup |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackGroup |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackGroup |= isHungry1 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackGroup |= isHungry1 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackGroup |= isStarving1 == true;
                            break;
                    }

                    // aggression group vs tamed

                    switch (attackByGroup2)
                    {
                        case IsEnemyCondition.Never:
                            canAttackGroup |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackGroup |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackGroup |= isHungry2 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackGroup |= isHungry2 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackGroup |= isStarving2 == true;
                            break;
                    }
                }

                if (isSameFaction)
                {
                    // aggression tamed vs faction

                    switch (attackFaction1)
                    {
                        case IsEnemyCondition.Never:
                            canAttackFaction |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackFaction |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackFaction |= isHungry1 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackFaction |= isHungry1 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackFaction |= isStarving1 == true;
                            break;
                    }

                    // aggression faction vs tamed

                    switch (attackByFaction2)
                    {
                        case IsEnemyCondition.Never:
                            canAttackFaction |= false;
                            break;
                        case IsEnemyCondition.Always:
                            canAttackFaction |= true;
                            break;
                        case IsEnemyCondition.WhenFed:
                            canAttackFaction |= isHungry2 == false;
                            break;
                        case IsEnemyCondition.WhenHungry:
                            canAttackFaction |= isHungry2 == true;
                            break;
                        case IsEnemyCondition.WhenStarving:
                            canAttackFaction |= isStarving2 == true;
                            break;
                    }
                }

                // important: group > faction|tamed

                if (isSameGroup && canAttackGroup && (isSameTamed == false || canAttackTamed))
                {
                    // group aggression allowed
                    // and not both are tamed OR tamed aggression allowed
                    canAttack |= true;
                }
                else if (isSameFaction && canAttackFaction && (isSameTamed == false || canAttackTamed))
                {
                    // group aggression allowed
                    // and not both are tamed OR tamed aggression allowed
                    canAttack |= true;
                }
                else if (isSameTamed && canAttackTamed)
                {
                    // not the same group or faction
                    // but both sides are tamed + aggression allowed
                    // use IsEnemyContext to temporarly disable tamed status of b (the target beeing attacked by a)
                    StaticContext.IsEnemyContext.Active = true;
                    StaticContext.IsEnemyContext.TargetInstance = b;
                }
                
            }

            if (canAttack)
            {
                __result = true;
                return false;
            }

            return true; // let valheim decide
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyFinalizer]
        private static void BaseAI_IsEnemy_Finalizer(Exception __exception)
        {
            StaticContext.IsEnemyContext.Cleanup();
        }

    }
}
