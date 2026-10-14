using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace SK.Events
{
    // Cerebro del Darkspawn: presa viva primero, luego cadaver cercano,
    // luego generador electrico, abriendo puertas por la fuerza si hace falta.
    // Alcance de caza 120 casillas (14400 al cuadrado).
    public class JobGiver_Darkness : ThinkNode_JobGiver
    {
        public const int SEARCH_DISTANCE = 14400;

        public override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 pos = pawn.Position;
            if (pawn.Map.fogGrid.IsFogged(pos))
            {
                return null;
            }
            TraverseParms traverse = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, true, false, false, false);
            // La presa viva va primero: con cadaveres en el mapa el bicho los
            // preferia siempre y pasaba de largo ante los vivos.
            Job hunt = TryGiveHuntJob(pawn, traverse);
            if (hunt != null)
            {
                return hunt;
            }
            Corpse corpse = FindMeatyCorpse(pawn, traverse);
            if (corpse != null && (pawn.jobs.curJob == null || (pawn.jobs.curJob.def != JobDefOfLocal.TakenInTheDark && pawn.jobs.curJob.checkOverrideOnExpire)) && corpse.InnerPawn.def.race.Humanlike && ReservationUtility.CanReserve(pawn, corpse, 1, -1, null, false))
            {
                return new Job(JobDefOfLocal.TakenInTheDark, corpse);
            }
            int roll = Rand.Range(1, 100);
            if (roll < 61 && !pawn.mindState.MeleeThreatStillThreat)
            {
                Building power = FindOpportunisticPower(pawn, traverse);
                if (power != null && ReservationUtility.CanReserve(pawn, power, 1, -1, null, false))
                {
                    return new Job(JobDefOf.AttackMelee, power);
                }
            }
            return null;
        }

        private static Job TryGiveHuntJob(Pawn pawn, TraverseParms traverse)
        {
            JobDef kill = JobDefOfLocal.Kill;
            if (pawn.jobs.curJob != null && (pawn.jobs.curJob.def == kill || !pawn.jobs.curJob.checkOverrideOnExpire))
            {
                return null;
            }
            Pawn prey = FindMeatyPrey(pawn, traverse);
            if (prey == null || !ReservationUtility.CanReserve(pawn, prey, 1, -1, null, false))
            {
                return null;
            }
            PawnPath path = pawn.Map.pathFinder.FindPathNow(pawn.Position, prey, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, true, false, false, false), null, PathEndMode.OnCell, null);
            try
            {
                IntVec3 cellBefore = IntVec3.Invalid;
                Thing blocker = PawnPathUtility.FirstBlockingBuilding(path, out cellBefore, pawn);
                if (blocker != null)
                {
                    Building_Door door = blocker as Building_Door;
                    if (door == null && ReservationUtility.CanReserve(pawn, blocker, 1, -1, null, false))
                    {
                        return new Job(JobDefOfLocal.CrushDoor, cellBefore, blocker)
                        {
                            maxNumMeleeAttacks = 4,
                            expiryInterval = 500
                        };
                    }
                    if (door != null && !door.Open && ReservationUtility.CanReserve(pawn, door, 1, -1, null, false))
                    {
                        return new Job(JobDefOfLocal.CrushDoor, cellBefore, door)
                        {
                            maxNumMeleeAttacks = 4,
                            expiryInterval = 500
                        };
                    }
                    return null;
                }
                return new Job(kill)
                {
                    targetA = prey,
                    attackDoorIfTargetLost = true,
                    maxNumMeleeAttacks = 4,
                    killIncappedTarget = true,
                    expiryInterval = 500
                };
            }
            finally
            {
                path?.Dispose();
            }
        }

        public static Corpse FindMeatyCorpse(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.Corpse);
            Thing found = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.Touch, traverseParams, 100f, corpse => pawn.Map.reservationManager.CanReserve(pawn, corpse, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false, false);
            return found as Corpse;
        }

        public static Pawn FindMeatyPrey(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            return (Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.AllPawnsSpawned, 300f, p => isPossiblePrey(p as Pawn, pawn), null, false);
        }

        public static Building FindOpportunisticPower(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            Thing found = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.Touch, traverseParams, 100f, b => b.def.EverTransmitsPower && !b.def.IsFrame && !b.def.IsBlueprint, null, 0, -1, false, RegionType.Set_Passable, false, false);
            return found as Building;
        }

        private static bool isPossiblePrey(Pawn prey, Pawn hunter)
        {
            if (prey == null || hunter == null || hunter == prey)
            {
                return false;
            }
            if (prey.Dead || prey.Downed)
            {
                return false;
            }
            if (!prey.RaceProps.Humanlike)
            {
                return false;
            }
            if (isFriendly(hunter, prey) || isAlly(hunter, prey) || isMech(hunter, prey) || isOwnRace(hunter, prey))
            {
                return false;
            }
            return isNearby(hunter, prey);
        }

        private static bool isFriendly(Pawn hunter, Pawn prey)
        {
            if (prey == null)
            {
                return false;
            }
            if (hunter.Faction == Faction.OfPlayer)
            {
                return prey.Faction == Faction.OfPlayer || prey.IsPrisonerOfColony || FactionUtility.HostileTo(prey.Faction, Faction.OfPlayer);
            }
            return prey.def == hunter.def;
        }

        private static bool isAlly(Pawn hunter, Pawn prey)
        {
            if (prey == null)
            {
                return false;
            }
            if (hunter.Faction != null && hunter.Faction == prey.Faction)
            {
                return true;
            }
            return prey.def == hunter.def;
        }

        private static bool isMech(Pawn hunter, Pawn prey)
        {
            if (prey == null)
            {
                return false;
            }
            if (prey.Faction == Faction.OfMechanoids)
            {
                return true;
            }
            return prey.def == hunter.def;
        }

        private static bool isOwnRace(Pawn hunter, Pawn prey)
        {
            if (prey == null)
            {
                return false;
            }
            if (hunter.RaceProps.Animal && prey.RaceProps.Animal)
            {
                return true;
            }
            return prey.def == hunter.def;
        }

        private static bool isHumanlike(Pawn hunter, Pawn prey)
        {
            if (prey == null)
            {
                return false;
            }
            if (prey.RaceProps.Humanlike)
            {
                return true;
            }
            return prey.def == hunter.def;
        }

        private static bool isNearby(Pawn pawn, Thing thing)
        {
            IntVec3 diff = pawn.Position - thing.Position;
            return diff.LengthHorizontalSquared <= SEARCH_DISTANCE;
        }
    }
}

