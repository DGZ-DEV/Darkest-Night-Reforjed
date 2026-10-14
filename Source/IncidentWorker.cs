using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SK.Events
{
    // Incidente Darkness: registra la condicion temporal y planta el contador
    // BVcounter que lleva la cuenta atras y el spawn. Si hay un Eclipse activo
    // lo retira antes, porque ambas condiciones se excluyen en el XML.
    public class IncidentWorker_BlackVoid : IncidentWorker_MakeGameCondition
    {
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map.GameConditionManager.ConditionIsActive(GameConditionDefOfLocal.Darkness))
            {
                return false;
            }
            if (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
            {
                // Se copia a una lista temporal: borrar mientras se itera
                // la lista viva lanza InvalidOperationException.
                List<GameCondition> active = new List<GameCondition>(map.GameConditionManager.ActiveConditions);
                foreach (GameCondition cond in active)
                {
                    if (cond.def == GameConditionDefOf.Eclipse)
                    {
                        map.GameConditionManager.OnConditionEnd(cond);
                        break;
                    }
                }
            }
            map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(GameConditionDefOfLocal.Darkness, 16000));
            Find.LetterStack.ReceiveLetter(Translator.Translate("LabelBV"), Translator.Translate("DescBV"), LetterDefOf.ThreatBig);
            IntVec3 cell = CellFinderLoose.RandomCellWith(sq => GenGrid.Standable(sq, map) && !map.roofGrid.Roofed(sq) && !map.fogGrid.IsFogged(sq), map, 1000);
            Thing counter = ThingMaker.MakeThing(ThingDefOfLocal.BVcounter);
            GenSpawn.Spawn(counter, cell, map, WipeMode.Vanish);
            return true;
        }
    }
}

