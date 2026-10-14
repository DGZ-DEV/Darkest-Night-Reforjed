using RimWorld;
using Verse;

namespace SK
{
    // Referencias tipadas a los Defs del mod. RimWorld las resuelve al cargar los XML.
    [DefOf]
    public static class FactionDefOfLocal
    {
        public static FactionDef Darkness;
    }

    [DefOf]
    public static class GameConditionDefOfLocal
    {
        public static GameConditionDef Darkness;

        public static GameConditionDef DarknessPermanent;
    }

    [DefOf]
    public static class JobDefOfLocal
    {
        public static JobDef TakenInTheDark;

        public static JobDef CrushDoor;

        public static JobDef Kill;
    }

    [DefOf]
    public static class PawnKindDefOfLocal
    {
        public static PawnKindDef Void;
    }

    [DefOf]
    public static class SongDefOfLocal
    {
        public static SongDef Darkest;
    }

    [DefOf]
    public static class SoundDefOfLocal
    {
        public static SoundDef Destroyed;

        public static SoundDef DarkestEndEvent;
    }

    [DefOf]
    public static class ThingDefOfLocal
    {
        public static ThingDef BVcounter;

        public static ThingDef Void;
    }

    [DefOf]
    public static class WeatherDefOfLocal
    {
        public static WeatherDef Dark;
    }
}
