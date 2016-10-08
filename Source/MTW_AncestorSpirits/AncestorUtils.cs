using Verse;
using RimWorld;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public static class AncestorUtils
    {
        public static int DaysToTicks(float days)
        {
            return Mathf.RoundToInt(days * GenDate.TicksPerDay);
        }

        public static int HoursToTicks(float hours)
        {
            return Mathf.RoundToInt(hours * GenDate.TicksPerHour);
        }

        public static long EstStartOfSeasonAt(long ticks)
        {
            var currentDayTicks = (int)(GenDate.CurrentDayPercent * GenDate.TicksPerDay);
            var dayOfSeason = GenDate.DayOfSeasonZeroBasedAt(ticks);
            var currentSeasonDayTicks = DaysToTicks(dayOfSeason);

            return ticks - currentDayTicks - currentSeasonDayTicks;
        }
    }
}
