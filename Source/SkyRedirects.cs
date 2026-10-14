using Rework;
using RimWorld;
using SK;
using SK.Events;
using UnityEngine;
using Verse;

namespace DarkestNight
{
    // Tinte del cielo sin Harmony. Rework reescribe en memoria las llamadas a
    // Lerp hacia estos metodos antes del arranque, asi que el juego ni se
    // entera del desvio. Dentro se puede llamar al original sin recursion.
    public static class SkyRedirects
    {
        private static readonly Color DarkSky = new Color(0.05f, 0.05f, 0.05f);

        private static bool PermanentActive()
        {
            Map map = Find.CurrentMap;
            return map != null && map.GameConditionManager.ConditionIsActive(GameConditionDefOfLocal.DarknessPermanent);
        }

        private static bool SunUp(Map map)
        {
            return GameCondition_DarknessPermanent.allowSunlight
                && GenCelestial.CelestialSunGlow(map, Find.TickManager.TicksAbs) >= 0.3f;
        }

        [ReworkRedirect(Type = "Verse.SkyColorSet", Method = "Lerp")]
        public static SkyColorSet SkyColorSetLerp(SkyColorSet A, SkyColorSet B, float t)
        {
            Map map = Find.CurrentMap;
            if (map != null && PermanentActive() && !SunUp(map))
            {
                return new SkyColorSet
                {
                    sky = DarkSky,
                    shadow = Color.white,
                    overlay = DarkSky,
                    saturation = 1f
                };
            }
            return SkyColorSet.Lerp(A, B, t);
        }

        [ReworkRedirect(Type = "Verse.SkyColorSet", Method = "LerpDarken")]
        public static SkyColorSet SkyColorSetLerpDarken(SkyColorSet A, SkyColorSet B, float t)
        {
            Map map = Find.CurrentMap;
            if (map != null && PermanentActive() && !SunUp(map))
            {
                return new SkyColorSet
                {
                    sky = DarkSky,
                    shadow = Color.white,
                    overlay = DarkSky,
                    saturation = 1f
                };
            }
            return SkyColorSet.LerpDarken(A, B, t);
        }

        [ReworkRedirect(Type = "Verse.SkyTarget", Method = "Lerp")]
        public static SkyTarget SkyTargetLerp(SkyTarget A, SkyTarget B, float t)
        {
            Map map = Find.CurrentMap;
            if (map != null && PermanentActive() && !SunUp(map))
            {
                return new SkyTarget
                {
                    colors = SkyColorSet.Lerp(A.colors, A.colors, 0f),
                    glow = 0f,
                    lightsourceShineSize = 0f,
                    lightsourceShineIntensity = 0f
                };
            }
            return SkyTarget.Lerp(A, B, t);
        }

        [ReworkRedirect(Type = "Verse.SkyTarget", Method = "LerpDarken")]
        public static SkyTarget SkyTargetLerpDarken(SkyTarget A, SkyTarget B, float t)
        {
            Map map = Find.CurrentMap;
            if (map != null && PermanentActive() && !SunUp(map))
            {
                return new SkyTarget
                {
                    colors = SkyColorSet.Lerp(A.colors, A.colors, 0f),
                    glow = 0f,
                    lightsourceShineSize = 0f,
                    lightsourceShineIntensity = 0f
                };
            }
            return SkyTarget.LerpDarken(A, B, t);
        }
    }
}
