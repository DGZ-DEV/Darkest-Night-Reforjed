using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SK.Events
{
    // Condicion temporal del incidente Darkness. Enfria el mapa y oscurece el cielo
    // mientras dura. El tinte del cielo lo aplica SkyRedirects segun esta condicion.
    public class GameCondition_Darkness : GameCondition
    {
        private int LerpTicks = 3750;

        private float MaxTempOffset = -10f;

        private SkyColorSet DarknessSkyColors = new SkyColorSet(new UnityEngine.Color(0.05f, 0.05f, 0.05f), UnityEngine.Color.white, new UnityEngine.Color(0.05f, 0.05f, 0.05f), 1f);

        public override float TemperatureOffset()
        {
            return GameConditionUtility.LerpInOutValue(TicksPassed, TicksLeft, LerpTicks, MaxTempOffset);
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(TicksPassed, TicksLeft, LerpTicks, 1f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(0f, DarknessSkyColors, 0f, 0f);
        }
    }

    // Condicion del escenario Permanent Darkness. Mantiene el frio y genera
    // Darkspawns cerca de colonos a oscuras. Los campos estaticos se comparten
    // entre mapas porque la condicion es unica por partida.
    public class GameCondition_DarknessPermanent : GameCondition
    {
        public static bool allowSunlight;

        private static bool SunlightDown;

        private static bool sunlightWarningGiven;

        private Pawn pawn = null;

        private int LerpTicks = 3750;

        private float MaxTempOffset = -10f;

        private static Faction OfDarkness => Find.FactionManager.FirstFactionOfDef(FactionDefOfLocal.Darkness);

        public override float TemperatureOffset()
        {
            return GameConditionUtility.LerpInOutValue(TicksPassed, LerpTicks, MaxTempOffset, 1f);
        }

        public override void Init()
        {
            base.Init();
            SunlightDown = false;
            sunlightWarningGiven = false;
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = AffectedMaps;
            if (Find.TickManager.TicksGame % 500 != 0)
            {
                return;
            }
            if (allowSunlight)
            {
                Map currentMap = Find.CurrentMap;
                if (currentMap == null)
                {
                    return;
                }
                float sunGlow = GenCelestial.CelestialSunGlow(currentMap, Find.TickManager.TicksAbs);
                if (!sunlightWarningGiven && sunGlow < 1f)
                {
                    Messages.Message(Translator.Translate("DarknessComesSunlight"), MessageTypeDefOf.NeutralEvent);
                    sunlightWarningGiven = true;
                }
                if (sunlightWarningGiven && sunGlow == 1f)
                {
                    sunlightWarningGiven = false;
                }
                if (sunGlow < 0.3f)
                {
                    SunlightDown = true;
                }
                else if (SunlightDown)
                {
                    // El sol volvio: se retiran los Darkspawns activos de este mapa.
                    // Se toma una foto de la lista porque Destroy modifica la coleccion.
                    List<Pawn> voids = (from Pawn pwn in currentMap.mapPawns.AllPawns
                                        where pwn.def == ThingDefOfLocal.Void
                                        select pwn).ToList();
                    foreach (Pawn v in voids)
                    {
                        v.Destroy(DestroyMode.Vanish);
                    }
                    SunlightDown = false;
                }
            }
            for (int i = 0; i < affectedMaps.Count; i++)
            {
                SummonDarkness(affectedMaps[i]);
            }
        }

        private static void GenVoid(Pawn pawn, IntVec3 intVec, Map map)
        {
            if ((int)map.glowGrid.PsychGlowAt(intVec) > 0 || !GenGrid.Walkable(intVec, map) || (int)RegionTypeUtility.GetExpectedRegionType(intVec, map) == 4)
            {
                return;
            }
            // En 1.6 GeneratePawn exige el tile: null mantiene el comportamiento 1.4.
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

        private void SummonDarkness(Map map)
        {
            if (allowSunlight && !(GenCelestial.CelestialSunGlow(map, Find.TickManager.TicksAbs) < 0.3f))
            {
                return;
            }
            List<Pawn> list = (from Pawn pwn in map.mapPawns.AllPawns
                               where pwn.def == ThingDefOfLocal.Void
                               select pwn).ToList();
            int count = list.Count();
            if (count < 20)
            {
                int roll = Rand.Range(1, 100);
                if (roll < 71)
                {
                    IReadOnlyList<Pawn> spawned = map.mapPawns.AllPawnsSpawned;
                    int placed = 0;
                    for (int i = 0; i < spawned.Count; i++)
                    {
                        if (placed >= 30)
                        {
                            break;
                        }
                        Pawn candidate = spawned[i];
                        if (candidate.RaceProps.Humanlike && (int)map.glowGrid.PsychGlowAt(candidate.Position) == 0)
                        {
                            IntVec3 cell = CellFinder.RandomClosewalkCellNear(candidate.Position, map, 3, null);
                            GenVoid(pawn, cell, map);
                            placed++;
                        }
                    }
                }
                else
                {
                    IntVec3 cell = CellFinderLoose.RandomCellWith(c => GenGrid.Standable(c, map) && !map.roofGrid.Roofed(c) && !map.fogGrid.IsFogged(c), map, 600);
                    GenVoid(pawn, cell, map);
                }
            }
            else
            {
                Pawn extra = GenCollection.RandomElement<Pawn>(list);
                extra.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
