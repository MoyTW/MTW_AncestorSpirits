using RimWorld;
using Verse;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_HeavyWind : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            WeatherDef weatherDef = DefDatabase<WeatherDef>.GetNamed("MTW_HeavyWind");
            Find.WeatherManager.TransitionTo(weatherDef);

            /* ##### HACK ALERT! #####
             * We force the weather to stay the same for extended periods through a watcher in the MapComponent. See
             * the Notify_ForceWeather function for details.
             */
            int weatherDuration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * GenDate.TicksPerDay);
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_ForceWeather(weatherDef, weatherDuration);

            return true;
        }
    }
}
