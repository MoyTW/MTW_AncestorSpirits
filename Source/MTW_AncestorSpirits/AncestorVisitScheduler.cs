using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class VisitItinerary : IExposable
    {
        public int NumVisitors;
        public long StartTick;
        public int DurationTicks;
        public long EndTick { get { return this.StartTick + this.DurationTicks; } }

        public VisitItinerary(int numVisitors, long startTick, int durationTicks)
        {
            this.NumVisitors = numVisitors;
            this.StartTick = startTick;
            this.DurationTicks = durationTicks;
        }

        public bool Overlaps(VisitItinerary otherItinerary, int bufferBetweenVisits)
        {
            return this.StartTick <= (otherItinerary.EndTick + bufferBetweenVisits) &&
                (this.EndTick + bufferBetweenVisits) >= otherItinerary.StartTick;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.NumVisitors, "NumVisitors");
            Scribe_Values.LookValue<long>(ref this.StartTick, "StartTick");
            Scribe_Values.LookValue<int>(ref this.DurationTicks, "DurationTicks");
        }

        public override string ToString()
        {
            return String.Format("{0} ancestors are visitng from {1} to {2}", this.NumVisitors,
                GenDate.DateReadoutStringAt((int)this.StartTick), GenDate.DateReadoutStringAt((int)this.EndTick));
        }
    }

    public static class AncestorVisitScheduler
    {
        private static readonly IntRange numVisitorsRange = new IntRange(AncestorConstants.ANCESTORS_PER_VISIT, AncestorConstants.ANCESTORS_PER_VISIT);
        private static readonly IntRange numVisitsRange = new IntRange(2, 4);

        private static readonly IntRange visitDurationRange = new IntRange(AncestorUtils.DaysToTicks(2.5f),
            AncestorUtils.DaysToTicks(3.5f));
        private static readonly int bufferSeasonEnd = AncestorUtils.HoursToTicks(12f);
        private static readonly int bufferBetweenVisits = AncestorUtils.HoursToTicks(6f);

        private static readonly IntRange startTickRangeRel =
            new IntRange(0, GenDate.TicksPerSeason - visitDurationRange.max - bufferSeasonEnd);

        public static List<VisitItinerary> BuildSeasonSchedule(long seasonStartTick)
        {
            int numVisits = numVisitsRange.RandomInRange;

            List<VisitItinerary> visitSchedule = new List<VisitItinerary>();
            int attempts = 0;
            while (visitSchedule.Count < numVisits && attempts < 500)
            {
                int numVisitors = numVisitorsRange.RandomInRange;
                int duration = visitDurationRange.RandomInRange;
                long start = seasonStartTick + startTickRangeRel.RandomInRange;

                VisitItinerary newItinerary = new VisitItinerary(numVisitors, start, duration);
                if (!visitSchedule.Any(i => i.Overlaps(newItinerary, bufferBetweenVisits)))
                {
                    visitSchedule.Add(newItinerary);
                }
            }

            return visitSchedule;
        }

    }
}
