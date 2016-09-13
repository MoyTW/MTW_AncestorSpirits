using RimWorld;
using Verse;
using UnityEngine;

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

            /* ##### HACK ALERT! #####
             *
             *  We set the curWeatherAge (which is inexplicably public) to a negative value, equal to the intended
             *  duration of the effet. This is to get around the fact that the value against which it's compared, the
             *  curWeatherDuration in WeatherDecider, is completely inaccessible except by starting a new weather
             *  cycle. This means that the effect will last a time equal to the set duration + the last weather
             *  effect's duration.
             *
             *  Note that the curWeatherAge will reset to 0 if it is forcibly transitioned.
             */
            int rainDuration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * GenDate.TicksPerDay);
            Find.WeatherManager.curWeatherAge -= rainDuration;

            return true;
        }
    }
}
