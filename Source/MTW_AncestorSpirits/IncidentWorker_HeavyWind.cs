﻿using RimWorld;
using Verse;

using System.Reflection;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_HeavyWind : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            // Force a transition
            WeatherDef weatherDef = DefDatabase<WeatherDef>.GetNamed("MTW_HeavyWind");
            Find.WeatherManager.TransitionTo(weatherDef);

            // Set the duration using reflection
            int weatherDuration = AncestorUtils.DaysToTicks(this.def.durationDays.RandomInRange);
            var decider = Find.Storyteller.weatherDecider;
            typeof(WeatherDecider)
                .GetField("curWeatherDuration", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(decider, weatherDuration);

            return true;
        }
    }
}
