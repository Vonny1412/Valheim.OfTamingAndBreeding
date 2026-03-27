using HarmonyLib;
using OfTamingAndBreeding.Components.Traits;
using static OfTamingAndBreeding.Components.Traits.CharacterTrait;

namespace OfTamingAndBreeding.Patches
{
    internal partial class DataReadyPatches
    {
        [HarmonyPatch(typeof(BaseAI), "IsEnemy", new[] { typeof(Character), typeof(Character) })]
        [HarmonyPrefix] // not using Last or First priority -> other mods may want to change behaviour
        private static bool BaseAI_IsEnemy_Prefix(Character a, Character b, bool __runOriginal, ref bool __result)
        {
            // blocked by different mod
            if (!__runOriginal)
            {
                return false;
            }

            if (!a || !b || a == b)
            {
                // identical behavior to vanilla
                __result = false;
                return false;
            }

            /*

            WARNING - this is a hot path!
            - return as early as possible
            - hostility is getting evaluated inside BaseAI.UpdateAI patch
            - hostilities are stored as bit masks to speed up rather than using multiple switches

            */

            bool isTamed1 = a.IsTamed();
            bool isTamed2 = b.IsTamed();
            var isOneTamed = isTamed1 || isTamed2;
            var isBothTamed = isTamed1 && isTamed2;

            // get traits only if needed
            CharacterTrait trait1 = null;
            CharacterTrait trait2 = null;
            if (isTamed1)
            {
                //trait1 = a.GetComponent<CharacterTrait>();
                trait1 = CharacterTrait.GetUnsafe(a.gameObject);
            }
            if (isTamed2)
            {
                //trait2 = b.GetComponent<CharacterTrait>();
                trait2 = CharacterTrait.GetUnsafe(b.gameObject);
            }

            Character.Faction faction1 = a.GetFaction();
            Character.Faction faction2 = b.GetFaction();

            bool isPlayer1 = faction1 == Character.Faction.Players;
            bool isPlayer2 = faction2 == Character.Faction.Players;
            var isOnePlayer = isPlayer1 || isPlayer2;

            bool aggra1 = a.GetBaseAI()?.IsAggravated() ?? false;
            bool aggra2 = b.GetBaseAI()?.IsAggravated() ?? false;
            // vanilla: aggravated creatures can become hostile towards players
            // always comes before otab logic - aggravated also beats "never"
            if (isOnePlayer && ((aggra1 && isPlayer2) || (aggra2 && isPlayer1)))
            {
                __result = true;
                return false;
            }

            string group1 = a.GetGroup();
            var isSameGroup = group1.Length > 0 && group1 == b.GetGroup();

            bool hasFactionAlliance;
            if (faction1 == faction2)
            {
                hasFactionAlliance = true;
            }
            else
            {
                // vanilla faction alliances
                // todo: make sure that the alliances are always up to date after any valheim update (keep this todo as reminder)
                hasFactionAlliance = faction1 switch
                {
                    Character.Faction.AnimalsVeg =>         false,
                    Character.Faction.PlayerSpawned =>      false,
                    Character.Faction.Players =>            faction2 == Character.Faction.Dverger,
                    Character.Faction.ForestMonsters =>     faction2 == Character.Faction.AnimalsVeg || faction2 == Character.Faction.Boss,
                    Character.Faction.Undead =>             faction2 == Character.Faction.Demon || faction2 == Character.Faction.Boss,
                    Character.Faction.Demon =>              faction2 == Character.Faction.Undead || faction2 == Character.Faction.Boss,
                    Character.Faction.MountainMonsters =>   faction2 == Character.Faction.Boss,
                    Character.Faction.SeaMonsters =>        faction2 == Character.Faction.Boss,
                    Character.Faction.PlainsMonsters =>     faction2 == Character.Faction.Boss,
                    Character.Faction.MistlandsMonsters =>  faction2 == Character.Faction.AnimalsVeg || faction2 == Character.Faction.Boss,
                    Character.Faction.Dverger =>            faction2 == Character.Faction.AnimalsVeg || faction2 == Character.Faction.Boss || faction2 == Character.Faction.Players,
                    Character.Faction.Boss =>               faction2 != Character.Faction.Players && faction2 != Character.Faction.PlayerSpawned,
                    Character.Faction.TrainingDummy =>      faction2 != Character.Faction.Players,
                    _ => false,
                };
            }

            // at least one side has to be tamed for otab logic
            // if none is tamed skip the while block and use vanilla
            if (!isOneTamed)
            {
                goto VANILLA;
            }

            var skipVanilla = false;
            var hostilityPlayer = CharacterTrait.HostilityMask.None;
            var hostilityWild = CharacterTrait.HostilityMask.None;
            var hostilityTamed = CharacterTrait.HostilityMask.None;
            var hostilityGroup = CharacterTrait.HostilityMask.None;
            var hostilityFaction = CharacterTrait.HostilityMask.None;

            //
            // build hostility bit masks
            // early return if any "Never" appears
            //

            // prioritice inter-faction
            // because most "Never" situations in otab are faction related
            if (hasFactionAlliance)
            {
                if (isTamed1)
                {
                    // tamed -> faction
                    hostilityFaction |= trait1.TamedCanAttackFaction;
                }
                if (isTamed2)
                {
                    // faction -> tamed
                    hostilityFaction |= trait2.TamedCanBeAttackedByFaction;
                }

                if ((hostilityFaction & HostilityMask.Never) != 0)
                {
                    __result = false;
                    return false;
                }
                if ((hostilityFaction & HostilityMask.Skip) != 0)
                {
                    skipVanilla = true;
                }
            }

            // inter-group hostility could also be "Never" many times
            if (isSameGroup)
            {
                if (isTamed1)
                {
                    // tamed -> group
                    hostilityGroup |= trait1.TamedCanAttackGroup;
                }
                if (isTamed2)
                {
                    // group -> tamed
                    hostilityGroup |= trait2.TamedCanBeAttackedByGroup;
                }

                if ((hostilityGroup & HostilityMask.Never) != 0)
                {
                    __result = false;
                    return false;
                }
                if ((hostilityGroup & HostilityMask.Skip) != 0)
                {
                    skipVanilla = true;
                }
            }

            // inter-tamed-hostility
            if (isBothTamed)
            {
                // tamed -> tamed (outgoing)
                hostilityTamed |= trait1.TamedCanAttackTamed;
                // tamed -> tamed (incoming)
                hostilityTamed |= trait2.TamedCanBeAttackedByTamed;

                if ((hostilityTamed & HostilityMask.Never) != 0)
                {
                    __result = false;
                    return false;
                }
                if ((hostilityTamed & HostilityMask.Skip) != 0)
                {
                    skipVanilla = true;
                }
            }
            // tamed vs wild / wild vs tamed
            else
            {
                // not both tamed, we only need to check each side on its own
                if (isTamed1)
                {
                    // tamed -> wild
                    hostilityWild |= trait1.TamedCanAttackWild;
                }
                else // isTamed2
                {
                    // wild -> tamed
                    hostilityWild |= trait2.TamedCanBeAttackedByWild;
                }

                if ((hostilityWild & HostilityMask.Never) != 0)
                {
                    __result = false;
                    return false;
                }
                if ((hostilityWild & HostilityMask.Skip) != 0)
                {
                    skipVanilla = true;
                }
            }

            if (isOnePlayer)
            {
                if (isPlayer1)
                {
                    if (isTamed2) // do not remove this. isplayer can also mean it is player-faction
                    {
                        // player -> tamed
                        hostilityPlayer |= trait2.TamedCanBeAttackedByPlayer;
                    }
                }
                else // isPlayer2
                {
                    if (isTamed1) // do not remove this. isplayer can also mean it is player-faction
                    {
                        // tamed -> player
                        hostilityPlayer |= trait1.TamedCanAttackPlayer;
                    }
                }

                if ((hostilityPlayer & HostilityMask.Never) != 0)
                {
                    __result = false;
                    return false;
                }
                if ((hostilityPlayer & HostilityMask.Skip) != 0)
                {
                    skipVanilla = true;
                }
            }

            //
            // evaluate hostility bit masks
            //

            var canAttackPlayer = (hostilityPlayer & HostilityMask.Attack) != 0;
            var canAttackGroup = (hostilityGroup & HostilityMask.Attack) != 0;
            var canAttackFaction = (hostilityFaction & HostilityMask.Attack) != 0;
            var canAttackTamed = (hostilityTamed & HostilityMask.Attack) != 0;
            var canAttackWild = (hostilityWild & HostilityMask.Attack) != 0;

            // player > group > faction > tamed > wild
            if (isOnePlayer)
            {
                // isPlayer ignores is-same-faction
                // isPlayer ignores is-both-tamed
                // isPlayer NOT ignores is-same-group
                if (canAttackPlayer && (!isSameGroup || canAttackGroup))
                {
                    __result = true;
                    return false;
                }
            }

            // group > faction > tamed > wild
            else if (isSameGroup)
            {
                // group aggression allowed
                // and not both are tamed OR tamed aggression allowed
                if (canAttackGroup && (!isBothTamed || canAttackTamed))
                {
                    __result = true;
                    return false;
                }
            }

            // faction > tamed > wild
            else if (hasFactionAlliance)
            {
                // faction aggression allowed
                // and not both are tamed OR tamed aggression allowed
                if (canAttackFaction && (!isBothTamed || canAttackTamed))
                {
                    __result = true;
                    return false;
                }
            }

            // tamed > wild
            else if (canAttackTamed)
            {
                // tame-vs-tame override by otab logic
                // set target (character b) creature tame state to not-tamed
                isTamed2 = false;
                isBothTamed = false;
                isOneTamed = isTamed1;
                // now we got tamed-vs-wild
                // continue with vanilla logic
            }

            // wild fallback
            else if (canAttackWild)
            {
                __result = true;
                return false;
            }

            // at least one otab hostility rule has been set
            // but none has clearly demanded an attack
            if (skipVanilla)
            {
                __result = false;
                return false;
            }
            
            //
            // vanilla logic
            //
            VANILLA:

            if (isSameGroup)
            {
                __result = false;
                return false;
            }

            if (isOneTamed)
            {
                if (isBothTamed
                || (isTamed1 && (isPlayer2 || (!aggra2 && faction2 == Character.Faction.Dverger)))
                || (isTamed2 && (isPlayer1 || (!aggra1 && faction1 == Character.Faction.Dverger)))
                )
                {
                    __result = false;
                    return false;
                }
                __result = true;
                return false;
            }

            // vanilla faction check
            __result = !hasFactionAlliance;
            return false;
        }

    }
}
