using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace SK.Events
{
    // Contador invisible que planta el incidente. Lleva la cuenta atras con
    // avisos, genera Darkspawns en la oscuridad y al terminar limpia y avisa.
    public class Ticker_BVcounter : Building
    {
        public static bool removeDarknessNarrator;

        private int start = 800;

        private int delay = 3400;

        private int gone = 7450;

        public bool mstarted = false;

        private Pawn pawn = null;

        private int spawninterval = 20;

        private static Faction OfDarkness => Find.FactionManager.FirstFactionOfDef(FactionDefOfLocal.Darkness);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref start, "Start", 800);
            Scribe_Values.Look(ref gone, "Gone", 7450);
            Scribe_Values.Look(ref delay, "Delay", 3400);
            Scribe_Values.Look(ref mstarted, "mstarted", false);
        }

        private static void GenVoid(Pawn pawn, IntVec3 intVec, Map map)
        {
            if ((int)map.glowGrid.PsychGlowAt(intVec) > 0 || !GenGrid.Walkable(intVec, map) || (int)RegionTypeUtility.GetExpectedRegionType(intVec, map) == 4)
            {
                return;
            }
            pawn = PawnGenerator.GeneratePawn(PawnKindDefOfLocal.Void, OfDarkness, null);
            if (ModsConfig.RoyaltyActive)
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("PsychicInvisibility"), pawn, null);
                HediffComp_Disappears disappears = HediffUtility.TryGetComp<HediffComp_Disappears>(hediff);
                if (disappears != null)
                {
                    disappears.ticksToDisappear = 60000;
                }
                pawn.health.AddHediff(hediff);
            }
            GenSpawn.Spawn(pawn, intVec, map, WipeMode.Vanish);
        }

        private void PlayDarkestSong()
        {
            // En 1.6 ForceStartSong paso a llamarse ForcePlaySong con la misma firma.
            Find.MusicManagerPlay.ForcePlaySong(SongDefOfLocal.Darkest, false);
        }

        public override void Tick()
        {
            base.Tick();
            if (!mstarted)
            {
                PlayDarkestSong();
                mstarted = true;
            }
            if (delay > 0)
            {
                delay--;
                if (delay == 2200)
                {
                    Messages.Message(Translator.Translate("DarknessComesA"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 1800)
                {
                    Messages.Message(Translator.Translate("DarknessComesB"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 900)
                {
                    Messages.Message(Translator.Translate("DarknessComesC"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 300)
                {
                    Messages.Message(Translator.Translate("DarknessComesD"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 240)
                {
                    Messages.Message(Translator.Translate("DarknessComesE"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 180)
                {
                    Messages.Message(Translator.Translate("DarknessComesF"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 120)
                {
                    Messages.Message(Translator.Translate("DarknessComesG"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 60)
                {
                    Messages.Message(Translator.Translate("DarknessComesH"), MessageTypeDefOf.NeutralEvent);
                }
                else if (delay == 0)
                {
                    Messages.Message(Translator.Translate("DarknessComes"), MessageTypeDefOf.NeutralEvent);
                }
            }
            if (delay != 0)
            {
                return;
            }
            if (!Find.MusicManagerPlay.IsPlaying)
            {
                PlayDarkestSong();
            }
            if (start > 0)
            {
                start--;
            }
            if (start == 0 && gone > 0)
            {
                gone--;
                Pawn victim = null;
                if (GenCollection.TryRandomElement(Map.mapPawns.AllPawns.Where(col => col.RaceProps.Humanlike && col.Spawned), out victim) && victim != null)
                {
                    List<Pawn> voids = (from Pawn pwn in Map.mapPawns.AllPawns
                                        where pwn.def == ThingDefOfLocal.Void
                                        select pwn).ToList();
                    if (voids.Count() <= 150)
                    {
                        spawninterval--;
                        float roll = Rand.Value;
                        if (spawninterval <= 0)
                        {
                            if (roll < 0.5f)
                            {
                                IntVec3 cell = CellFinderLoose.RandomCellWith(c => GenGrid.Standable(c, Map) && !Map.roofGrid.Roofed(c) && !Map.fogGrid.IsFogged(c), Map, 600);
                                GenVoid(pawn, cell, Map);
                                spawninterval = 20;
                            }
                            else if (roll <= 0.7f)
                            {
                                IntVec3 cell = CellFinder.RandomClosewalkCellNear(victim.Position, Map, 6, null);
                                GenVoid(pawn, cell, Map);
                                spawninterval = 20;
                            }
                            else
                            {
                                List<Building> shelters = (from Building g in Map.listerBuildings.allBuildingsColonist
                                                           where !g.def.holdsRoof && !g.def.IsFrame && !g.def.IsDoor && !g.def.IsBlueprint
                                                           select g).ToList();
                                if (shelters.Count > 0)
                                {
                                    Building anchor = GenCollection.RandomElement<Building>(shelters);
                                    IntVec3 cell = CellFinder.RandomClosewalkCellNear(anchor.Position, Map, 3, null);
                                    GenVoid(pawn, cell, Map);
                                }
                                else
                                {
                                    IntVec3 cell = CellFinderLoose.RandomCellWith(c => GenGrid.Standable(c, Map) && !Map.roofGrid.Roofed(c) && !Map.fogGrid.IsFogged(c), Map, 600);
                                    GenVoid(pawn, cell, Map);
                                }
                                spawninterval = 20;
                            }
                        }
                    }
                }
            }
            if (gone != 0)
            {
                return;
            }
            List<Pawn> left = (from Pawn pwn in Map.mapPawns.AllPawns
                               where pwn.def == ThingDefOfLocal.Void
                               select pwn).ToList();
            int remaining = left.Count();
            if (remaining > 0)
            {
                Pawn one = GenCollection.RandomElement<Pawn>(left);
                one.Destroy(DestroyMode.Vanish);
            }
            if (remaining == 0)
            {
                List<Pawn> colonists = (from Pawn pwn in Map.mapPawns.AllPawns
                                        where pwn.IsColonist
                                        select pwn).ToList();
                if (colonists.Count() > 0f && !removeDarknessNarrator)
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOfLocal.DarkestEndEvent);
                }
                Find.LetterStack.ReceiveLetter(Translator.Translate("OmniousDarkLabel"), Translator.Translate("OmniousDarkDesc"), LetterDefOf.NeutralEvent);
                // En 1.6 los SongDef de gameplay no tienen defName propio: si este
                // deja de existir, se sigue sin musica en vez de lanzar excepcion.
                SongDef calm = DefDatabase<SongDef>.GetNamed("Noodle_Planetfall_a", false);
                if (calm != null)
                {
                    Find.MusicManagerPlay.ForcePlaySong(calm, false);
                }
                Destroy(DestroyMode.Vanish);
            }
        }
    }
}
