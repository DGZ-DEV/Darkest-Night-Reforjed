using RimWorld;
using Verse;
using Verse.AI;

namespace SK.Events
{
    // Cuerpo del Darkspawn. El constructor base de Pawn ya crea los trackers,
    // asi que aqui solo queda el comportamiento: huir nunca, morir al caer,
    // morir con luz y morir al bajar del 40% de salud.
    internal class Pawn_void : Pawn
    {
        private int voidaway = 50;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref voidaway, "voidaway", 50);
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                TickRare();
            }
            // Se usa el mapa propio, no Find.CurrentMap: con el mapamundi abierto
            // CurrentMap es null y el original reventaba aqui.
            if (!Spawned || Map == null || !GenGrid.InBounds(Position, Map))
            {
                return;
            }
            voidaway--;
            mindState.mentalStateHandler.neverFleeIndividual = true;
            if (Downed || health.Downed || health.InPainShock)
            {
                Destroy(DestroyMode.Vanish);
                return;
            }
            if (!stances.FullBodyBusy)
            {
                try
                {
                    pather.PatherTick();
                }
                catch (System.Exception)
                {
                }
            }
            health.HealthTick();
            stances.StanceTrackerTick();
            if (equipment != null)
            {
                equipment.EquipmentTrackerTick();
            }
            if (jobs != null)
            {
                jobs.JobTrackerTick();
            }
            if (Downed || Dead)
            {
                voidaway--;
                if (voidaway <= 0)
                {
                    Destroy(DestroyMode.Vanish);
                    return;
                }
            }
            if ((int)Map.glowGrid.PsychGlowAt(Position) > 0)
            {
                voidaway--;
                if (voidaway <= 0)
                {
                    Destroy(DestroyMode.Vanish);
                    return;
                }
            }
            if (health.summaryHealth.SummaryHealthPercent < 0.4f || Dead)
            {
                DeSpawn(DestroyMode.Vanish);
            }
        }
    }
}
