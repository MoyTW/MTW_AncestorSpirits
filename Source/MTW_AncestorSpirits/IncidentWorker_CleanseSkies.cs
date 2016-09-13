using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_CleanseSkies : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            // How OP is this thing? Extremely OP. Will balance later.
            List<MapCondition> conditions = Find.MapConditionManager.ActiveConditions.ToList<MapCondition>();
            foreach (var condition in conditions)
            {
                if (!condition.Permanent)
                {
                    condition.End();
                }
            }

            Find.WeatherManager.TransitionTo(WeatherDefOf.Clear);

            return true;
        }
    }
}
