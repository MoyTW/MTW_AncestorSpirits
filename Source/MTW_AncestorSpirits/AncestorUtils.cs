using Verse;
using RimWorld;
using UnityEngine;

namespace MTW_AncestorSpirits
{
    public static class AncestorUtils
    {
        public const int TicksPerInterval = GenDate.TicksPerHour / 10;
        public const int IntervalsPerDay = GenDate.TicksPerDay / TicksPerInterval;
        public const int IntervalsPerSeason = GenDate.TicksPerSeason / TicksPerInterval;

        public static bool IsIntervalTick()
        {
            return (Find.TickManager.TicksGame % TicksPerInterval == 0);
        }

        public static float DayValueToIntervalValue(float dayValue)
        {
            return dayValue / IntervalsPerDay;
        }

        public static float SeasonValueToIntervalValue(float seasonValue)
        {
            return seasonValue / IntervalsPerSeason;
        }

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

        public static bool IsAncestor(Pawn p)
        {
            return p.def.defName == "Spirit";
        }
    }
}
