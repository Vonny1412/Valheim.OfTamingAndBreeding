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

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();
            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // both are wild => let valheim decide
            if (isTamed1 == isTamed2 && !isTamed1)
            {
                return true;
            }

            var custom1 = a.GetComponent<OTABCreature>();
            var custom2 = b.GetComponent<OTABCreature>();

            // handle stick-to-faction
            var stickToFaction1 = custom1 != null && custom1.m_stickToFaction;
            var stickToFaction2 = custom2 != null && custom2.m_stickToFaction;
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

            var tameableTrait1 = a.GetComponent<TameableTrait>();
            var tameableTrait2 = b.GetComponent<TameableTrait>();
            var isStarving1 = tameableTrait1 && tameableTrait1.IsStarving();
            var isStarving2 = tameableTrait2 && tameableTrait2.IsStarving();

            var attackTamed1 = custom1?.m_tamedCanAttackTamed ?? IsEnemyCondition.Never;
            var attackByTamed2 = custom2?.m_tamedCanBeAttackedByTamed ?? IsEnemyCondition.Never;
            var attackPlayer1 = custom1?.m_tamedCanAttackPlayer ?? IsEnemyCondition.Never;
            var attackByPlayer2 = custom2?.m_tamedCanBeAttackedByPlayer ?? IsEnemyCondition.Never;

            if (isTamed1 && isTamed2)
            {
                var canAttack = false;

                switch (attackTamed1)
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
                    switch (attackByTamed2)
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
                    StaticContext.IsEnemyContext.Active = true;
                    StaticContext.IsEnemyContext.TargetInstance = b;
                }

            }
            else if (isTamed1 && b.IsPlayer())
            {
                var canAttack = false;

                switch (attackPlayer1)
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
            else if (isTamed2 && a.IsPlayer())
            {
                var canAttack = false;

                switch (attackByPlayer2)
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

                if (canAttack)
                {
                    __result = true;
                    return false;
                }
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
