using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_FoggyRain : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Find.WeatherManager.TransitionTo(DefDatabase<WeatherDef>.GetNamed("FoggyRain"));
            return true;
        }
    }
}
