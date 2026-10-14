using SK.Events;
using RimWorld;
using UnityEngine;
using Verse;

namespace DarkestNight
{
    // Ajustes del mod. Se aplican cuando los Defs ya estan cargados, no en
    // ReworkInit, porque en esa fase DefDatabase aun esta vacia.
    public class DarknessModSettings : ModSettings
    {
        public float baseChance = 3.5f;

        public float minRefireDays = 45f;

        public int earliestDay = 60;

        public bool allowSunlight = false;

        public bool removeDarknessNarrator = false;

        public bool darknessIgnoreDowned = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref baseChance, "baseChance", 3.5f);
            Scribe_Values.Look(ref minRefireDays, "minRefireDays", 45f);
            Scribe_Values.Look(ref earliestDay, "earliestDay", 60);
            Scribe_Values.Look(ref allowSunlight, "allowSunlight", false);
            Scribe_Values.Look(ref removeDarknessNarrator, "removeDarknessNarrator", false);
            Scribe_Values.Look(ref darknessIgnoreDowned, "darknessIgnoreDowned", false);
            base.ExposeData();
        }
    }

    public class DarknessSettings : Mod
    {
        public readonly DarknessModSettings settings;

        public DarknessSettings(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<DarknessModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.Label(Translator.Translate("DarknessBaseChanceLabel"));
            list.Gap(4f);
            string chance = settings.baseChance.ToString();
            list.TextFieldNumeric(ref settings.baseChance, ref chance, 0f, 100f);
            list.Gap(4f);
            list.Label(Translator.Translate("DarknessMinRefireDaysLabel"));
            list.Gap(4f);
            string refire = settings.minRefireDays.ToString();
            list.TextFieldNumeric(ref settings.minRefireDays, ref refire, 0f, 1000f);
            list.Gap(4f);
            list.Label(Translator.Translate("DarknessEarliestDayLabel"));
            list.Gap(4f);
            string earliest = settings.earliestDay.ToString();
            list.TextFieldNumeric(ref settings.earliestDay, ref earliest, 0f, 1000f);
            list.Gap(4f);
            list.CheckboxLabeled(Translator.Translate("DarknessAllowSunlightLabel"), ref settings.allowSunlight);
            list.Gap(4f);
            list.CheckboxLabeled(Translator.Translate("RemoveDarknessNarratorLabel"), ref settings.removeDarknessNarrator);
            list.Gap(4f);
            list.CheckboxLabeled(Translator.Translate("DarknessIgnoreDownedLabel"), ref settings.darknessIgnoreDowned);
            list.End();
            base.DoSettingsWindowContents(inRect);
            ApplyToDefs();
        }

        public override string SettingsCategory()
        {
            return "Darkest Night Reforjed";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplyToDefs();
        }

        private void ApplyToDefs()
        {
            IncidentDef darkness = DefDatabase<IncidentDef>.GetNamed("Darkness", false);
            if (darkness != null)
            {
                darkness.baseChance = settings.baseChance;
                darkness.minRefireDays = settings.minRefireDays;
                darkness.earliestDay = settings.earliestDay;
            }
            GameCondition_DarknessPermanent.allowSunlight = settings.allowSunlight;
            Ticker_BVcounter.removeDarknessNarrator = settings.removeDarknessNarrator;
            JobDriver_TakenInTheDark.removeDarknessNarrator = settings.removeDarknessNarrator;
            JobDriver_Kill.darknessIgnoreDowned = settings.darknessIgnoreDowned;
        }
    }

    // Aplica los ajustes guardados al arrancar, cuando los Defs ya existen.
    [StaticConstructorOnStartup]
    public static class SettingsImplementer
    {
        static SettingsImplementer()
        {
            DarknessSettings mod = LoadedModManager.GetMod<DarknessSettings>();
            if (mod == null)
            {
                return;
            }
            IncidentDef darkness = DefDatabase<IncidentDef>.GetNamed("Darkness", false);
            if (darkness != null)
            {
                darkness.baseChance = mod.settings.baseChance;
                darkness.minRefireDays = mod.settings.minRefireDays;
                darkness.earliestDay = mod.settings.earliestDay;
            }
            GameCondition_DarknessPermanent.allowSunlight = mod.settings.allowSunlight;
            Ticker_BVcounter.removeDarknessNarrator = mod.settings.removeDarknessNarrator;
            JobDriver_TakenInTheDark.removeDarknessNarrator = mod.settings.removeDarknessNarrator;
            JobDriver_Kill.darknessIgnoreDowned = mod.settings.darknessIgnoreDowned;
        }
    }
}
