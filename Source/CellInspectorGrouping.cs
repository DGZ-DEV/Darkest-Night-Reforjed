using System.Collections.Generic;
using Rework;
using RimWorld;
using UnityEngine;
using Verse;

namespace DarkestNight
{
    // Agrupa las filas repetidas del inspector de celda ("Peon oscuro x20").
    // El inspector vanilla dibuja una fila por objeto de la celda
    // (CellInspectorDrawer.DrawThingRow), y con 20 Voids apilados la ventana
    // se vuelve una lista infinita. Aqui se dibuja una sola fila con " xN"
    // para la primera ocurrencia de cada etiqueta y se saltan las demas.
    // Las etiquetas unicas (colonos con nombre, etc.) van por el original.
    public static class CellInspectorGrouping
    {
        private static int builtFrame = -1;

        private static readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        private static readonly Dictionary<string, int> firstId = new Dictionary<string, int>();

        [ReworkRedirect(Type = "Verse.CellInspectorDrawer", Method = "DrawThingRow")]
        public static void DrawThingRowGrouped(Thing thing)
        {
            try
            {
                int frame = Time.frameCount;
                if (frame != builtFrame)
                {
                    Rebuild();
                    builtFrame = frame;
                }
                Thing target = ResolveProxy(thing);
                string key = target?.LabelMouseover;
                if (key != null && counts.TryGetValue(key, out int count) && count > 1)
                {
                    // Solo la primera ocurrencia dibuja (" xN"); las demas se saltan.
                    // Es determinista por llamada, igual que el conteo vanilla.
                    if (firstId.TryGetValue(key, out int id) && target.thingIDNumber == id)
                    {
                        DrawGroupedRow(target, key, count);
                    }
                    return;
                }
            }
            catch
            {
                // Si algo falla, se dibuja vanilla: el inspector nunca se rompe.
            }
            CellInspectorDrawer.DrawThingRow(thing);
        }

        private static void Rebuild()
        {
            counts.Clear();
            firstId.Clear();
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }
            IntVec3 cell = UI.MouseCell();
            if (!cell.InBounds(map))
            {
                return;
            }
            // Mismo filtro que el inspector vanilla: sin motas, mugre, etereos
            // ni peones ocultos.
            foreach (Thing t in cell.GetThingList(map))
            {
                if (t?.def == null)
                {
                    continue;
                }
                ThingCategory cat = t.def.category;
                if (cat == ThingCategory.Mote || cat == ThingCategory.Filth || cat == ThingCategory.Ethereal)
                {
                    continue;
                }
                if (t is Pawn p && InvisibilityUtility.IsHiddenFromPlayer(p))
                {
                    continue;
                }
                Thing target = ResolveProxy(t) ?? t;
                string label = target.LabelMouseover;
                if (label == null)
                {
                    continue;
                }
                if (counts.TryGetValue(label, out int c))
                {
                    counts[label] = c + 1;
                }
                else
                {
                    counts[label] = 1;
                    firstId[label] = target.thingIDNumber;
                }
            }
        }

        private static Thing ResolveProxy(Thing thing)
        {
            if (thing == null)
            {
                return null;
            }
            CompSelectProxy proxy = thing.TryGetComp<CompSelectProxy>();
            if (proxy != null && proxy.thingToSelect != null)
            {
                return proxy.thingToSelect;
            }
            return thing;
        }

        // Misma fila que vanilla (icono, resaltado, etiqueta con elipses)
        // mas el sufijo " xN". numLines es privado: se abre con Publicizer.
        private static void DrawGroupedRow(Thing thing, string label, int count)
        {
            int line = CellInspectorDrawer.numLines;
            List<object> selected = Find.Selector.SelectedObjects;
            Rect row = new Rect(12f, line * 24f + 12f, 312f, 24f);
            if (selected.Contains(thing))
            {
                Widgets.DrawHighlight(row);
            }
            else if (line % 2 == 1)
            {
                Widgets.DrawLightHighlight(row);
            }
            Rect icon = new Rect(24f, line * 24f + 13f, 22f, 22f);
            if (thing is Blueprint || thing is Frame)
            {
                Widgets.DefIcon(icon, thing.def);
            }
            else if (thing is Pawn || thing is Corpse)
            {
                Widgets.ThingIcon(icon.ExpandedBy(5f), thing);
            }
            else
            {
                float scale;
                float angle;
                Vector2 proportions;
                Color color;
                Material material;
                if (Widgets.GetIconFor(thing, new Vector2(icon.width, icon.height), null, false, out scale, out angle, out proportions, out color, out material) != BaseContent.BadTex)
                {
                    Widgets.ThingIcon(icon, thing);
                }
            }
            Widgets.LabelEllipses(new Rect(58f, line * 24f + 12f, 370f, 24f), label + " x" + count);
            CellInspectorDrawer.numLines = line + 1;
        }
    }
}
