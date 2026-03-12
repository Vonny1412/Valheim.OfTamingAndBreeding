using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using OfTamingAndBreeding.Data.Models.SubData;
using System;
using System.Runtime.CompilerServices;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EvaluateCondition(
            IsEnemyCondition condition,
            bool relevant,
            bool isHungry,
            bool isStarving,
            ref bool neverEver)
        {
            switch (condition)
            {
                case IsEnemyCondition.Never:
                    return false;
                case IsEnemyCondition.Always:
                    return true;
                case IsEnemyCondition.WhenFed:
                    return !isHungry;
                case IsEnemyCondition.WhenHungry:
                    return isHungry;
                case IsEnemyCondition.WhenStarving:
                    return isStarving;
                case IsEnemyCondition.NeverEver:
                    neverEver = true;
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasFactionAlliance(Character.Faction a, Character.Faction b)
        {
            if (a == b)
                return true;

            if (a > b)
            {
#pragma warning disable IDE0180
                var tmp = a;
#pragma warning restore IDE0180
                a = b;
                b = tmp;
            }

            /* // current enum
            public enum Faction {
                Players,
                AnimalsVeg,
                ForestMonsters,
                Undead,
                Demon,
                MountainMonsters,
                SeaMonsters,
                PlainsMonsters,
                Boss,
                MistlandsMonsters,
                Dverger,
                PlayerSpawned,
                TrainingDummy
            }
            */

            switch (a)
            {
                case Character.Faction.Players:
                    return b == Character.Faction.Dverger;

                case Character.Faction.AnimalsVeg:
                    return b == Character.Faction.ForestMonsters
                        || b == Character.Faction.MistlandsMonsters
                        || b == Character.Faction.Dverger;

                case Character.Faction.Undead:
                    return b == Character.Faction.Demon
                        || b == Character.Faction.Boss;

                case Character.Faction.Demon:
                    return b == Character.Faction.Boss;

                case Character.Faction.ForestMonsters:
                case Character.Faction.MountainMonsters:
                case Character.Faction.SeaMonsters:
                case Character.Faction.PlainsMonsters:
                    return b == Character.Faction.Boss;

                case Character.Faction.Boss:
                    return b == Character.Faction.MistlandsMonsters
                        || b == Character.Faction.Dverger;
            }

            return false;
        }

        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyPrefix]
        // not using Last or First priority -> other mods may want to change behaviour
        private static bool BaseAI_IsEnemy_Prefix(Character a, Character b, ref bool __result)
        {
            StaticContext.IsEnemyContext.Depth++;
            bool isOuterMost = (StaticContext.IsEnemyContext.Depth == 1);
            if (!isOuterMost)
            {
                return true;
            }

            if (a == b || !a || !b)
            {
                return true;
            }

            bool isPlayer1 = a.IsPlayer();
            bool isPlayer2 = b.IsPlayer();
            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();

            // (both wild || both player) let valheim decide
            if ((!isTamed1 && !isTamed2) || (isPlayer1 && isPlayer2))
            {
                return true;
            }

            var trait1 = a.GetComponent<CharacterTrait>();
            var trait2 = b.GetComponent<CharacterTrait>();

            var isHungry1 = trait1 && trait1.IsHungry();
            var isHungry2 = trait2 && trait2.IsHungry();
            var isStarving1 = isHungry1 && trait1.IsStarving();
            var isStarving2 = isHungry2 && trait2.IsStarving();

            var neverEver = false;
            var canAttack = false;

            if (isPlayer2)
            {
                // aggression tamed vs player
                canAttack |= EvaluateCondition(trait1.m_canAttackPlayer, isTamed1, isHungry1, isStarving1, ref neverEver);
            }
            else if (isPlayer1)
            {
                // aggression player vs tamed
                canAttack |= EvaluateCondition(trait2.m_canBeAttackedByPlayer, isTamed2, isHungry2, isStarving2, ref neverEver);
            }
            else
            {
                Character.Faction faction1 = a.GetFaction();
                Character.Faction faction2 = b.GetFaction();
                string group1 = a.GetGroup();
                string group2 = b.GetGroup();

                var isBothTamed = isTamed1 && isTamed2;
                var isSameGroup = !string.IsNullOrEmpty(group1) && group1 == group2;
                var hasFactionAlliance = HasFactionAlliance(faction1, faction2);
                var canAttackTamed = false;
                var canAttackGroup = false;
                var canAttackFaction = false;

                // aggression tamed vs tamed (outgoing)
                canAttackTamed |= EvaluateCondition(trait1.m_canAttackTamed, isBothTamed, isHungry1, isStarving1, ref neverEver);

                // aggression tamed vs tamed (incoming)
                canAttackTamed |= EvaluateCondition(trait2.m_canBeAttackedByTamed, isBothTamed, isHungry2, isStarving2, ref neverEver);

                // aggression tamed vs group
                canAttackGroup |= EvaluateCondition(trait1.m_canAttackGroup, isTamed1 && isSameGroup, isHungry1, isStarving1, ref neverEver);

                // aggression group vs tamed
                canAttackGroup |= EvaluateCondition(trait2.m_canBeAttackedByGroup, isTamed2 && isSameGroup, isHungry2, isStarving2, ref neverEver);

                // aggression tamed vs faction
                canAttackFaction |= EvaluateCondition(trait1.m_canAttackFaction, isTamed1 && hasFactionAlliance, isHungry1, isStarving1, ref neverEver);

                // aggression faction vs tamed
                canAttackFaction |= EvaluateCondition(trait2.m_canBeAttackedByFaction, isTamed2 && hasFactionAlliance, isHungry2, isStarving2, ref neverEver);

                if (isSameGroup && canAttackGroup)
                {
                    // group aggression allowed
                    if (isBothTamed == false || canAttackTamed)
                    {
                        // and not both are tamed OR tamed aggression allowed
                        canAttack = true;
                    }
                }
                else if (hasFactionAlliance && canAttackFaction)
                {
                    // faction aggression allowed
                    if (isBothTamed == false || canAttackTamed)
                    {
                        // and not both are tamed OR tamed aggression allowed
                        canAttack = true;
                    }
                }
                else if (isBothTamed && canAttackTamed)
                {
                    // not the same group or faction
                    // but both sides are tamed + aggression allowed
                    // use IsEnemyContext to temporarly disable tamed status of b (the target beeing attacked by a)
                    StaticContext.IsEnemyContext.Active = true;
                    StaticContext.IsEnemyContext.TargetInstance = b;
                    //hint: if neverEver is true the patch finalizer will clear the context states anyway
                }

            }

            if (neverEver)
            {
                // neverever is a special case - always!
                // it should only be used to make creatures passive all the time
                // useful for animals like deers
                __result = false;
                return false;
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
