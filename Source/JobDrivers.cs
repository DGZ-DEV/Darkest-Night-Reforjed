using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SK.Events
{
    // El Darkspawn rompe la puerta que le bloquea el paso hacia la presa.
    internal class JobDriver_CrushDoor : JobDriver
    {
        private int numMeleeAttacksLanded;

        public TargetIndex a = TargetIndex.A;

        public TargetIndex b = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn actor = pawn;
            Job curJob = job;
            LocalTargetInfo cell = curJob.GetTarget(a);
            LocalTargetInfo door = curJob.GetTarget(b);
            // En 1.6 Reserve lleva el octavo parametro ignoreOtherReservations.
            return ReservationUtility.Reserve(actor, cell, curJob, 1, -1, null, errorOnFailed, false)
                && ReservationUtility.Reserve(actor, door, curJob, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            Toil go = Toils_Goto.GotoCell(a, PathEndMode.OnCell);
            ToilFailConditions.FailOnDestroyedOrNull(go, b);
            yield return go;
            yield return BashIt();
        }

        public Toil BashIt()
        {
            Toil bashIt = new Toil();
            bashIt.tickAction = delegate
            {
                Pawn actor = bashIt.actor;
                Job curJob = actor.jobs.curJob;
                Thing door = curJob.GetTarget(b).Thing;
                if (door == null || !door.Spawned)
                {
                    return;
                }
                if (actor.meleeVerbs.TryMeleeAttack(door, null, false))
                {
                    numMeleeAttacksLanded++;
                    if (numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            };
            bashIt.defaultCompleteMode = ToilCompleteMode.Never;
            ToilFailConditions.EndOnDespawnedOrNull(bashIt, b, JobCondition.Succeeded);
            ToilFailConditions.FailOn(bashIt, hunterIsKilled);
            return bashIt;
        }

        private bool hunterIsKilled()
        {
            return pawn.Dead;
        }
    }

    // Persigue a la presa y la golpea hasta derribarla. El remate lo decide
    // killIncappedTarget que pone el JobGiver al emitir el trabajo.
    public class JobDriver_Kill : JobDriver
    {
        public static bool darknessIgnoreDowned;

        private int numMeleeAttacksLanded;

        public TargetIndex a = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return ReservationUtility.Reserve(pawn, job.GetTarget(a), job, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            Toil followAndAttack = new Toil();
            followAndAttack.tickAction = delegate
            {
                Pawn actor = followAndAttack.actor;
                Job curJob = actor.jobs.curJob;
                Thing prey = curJob.GetTarget(a).Thing;
                if (prey == null || !prey.Spawned)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                Pawn victim = prey as Pawn;
                if (!(pawn.pather.Moving) && !GenAdj.AdjacentTo8WayOrInside(pawn.Position, prey))
                {
                    actor.pather.StartPath(prey, PathEndMode.Touch);
                }
                else if (GenAdj.AdjacentTo8WayOrInside(pawn.Position, prey))
                {
                    if (victim != null && victim.Downed && darknessIgnoreDowned)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                    if (actor.meleeVerbs.TryMeleeAttack(prey, null, false))
                    {
                        numMeleeAttacksLanded++;
                        if (numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                        {
                            EndJobWith(JobCondition.Succeeded);
                        }
                    }
                }
                else if (actor.pather.Destination.Thing != prey)
                {
                    actor.pather.StartPath(prey, PathEndMode.Touch);
                }
            };
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            ToilFailConditions.EndOnDespawnedOrNull(followAndAttack, a, JobCondition.Succeeded);
            ToilFailConditions.FailOn(followAndAttack, hunterIsKilled);
            yield return followAndAttack;
        }

        private bool hunterIsKilled()
        {
            return pawn.Dead;
        }
    }

    // Recoge el cadaver y lo hace desaparecer en la oscuridad, con su mensaje.
    public class JobDriver_TakenInTheDark : JobDriver
    {
        public static bool removeDarknessNarrator;

        public TargetIndex a = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return ReservationUtility.Reserve(pawn, job.GetTarget(a), job, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(a);
            this.FailOnDespawnedOrNull(a);
            this.FailOn(eaterIsKilled);
            Toil reserveCorpse = new Toil();
            reserveCorpse.initAction = delegate
            {
                Pawn actor = reserveCorpse.actor;
                Thing corpse = actor.CurJob.GetTarget(a).Thing;
                if (!corpse.Spawned || !Map.reservationManager.CanReserve(actor, corpse, 1, -1, null, false))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Map.reservationManager.Reserve(actor, actor.CurJob, corpse, 1, -1, null, true);
                }
            };
            reserveCorpse.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return reserveCorpse;
            yield return Toils_Goto.GotoThing(a, PathEndMode.Touch);
            yield return Toils_General.Wait(100);
            Toil chewCorpse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            chewCorpse.initAction = doChewCorpse;
            ToilFailConditions.EndOnDespawnedOrNull(chewCorpse, TargetIndex.A, JobCondition.Incompletable);
            yield return chewCorpse;
        }

        private bool eaterIsKilled()
        {
            return pawn.Dead;
        }

        private void doChewCorpse()
        {
            Corpse corpse = TargetThingA as Corpse;
            Pawn inner = corpse?.InnerPawn;
            if (corpse != null && inner != null)
            {
                // Faction y Name pueden ser null (pawns sin faccion o sin nombre):
                // el original reventaba aqui con NRE.
                if ((inner.Faction?.IsPlayer ?? false) && !removeDarknessNarrator)
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOfLocal.Destroyed, Map);
                }
                string name = inner.Name?.ToStringShort ?? inner.LabelShort;
                Messages.Message(Translator.Translate("TakenDC") + name + Translator.Translate("IntoDS"), MessageTypeDefOf.SilentInput);
                corpse.Destroy(DestroyMode.Vanish);
            }
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        }
    }
}

