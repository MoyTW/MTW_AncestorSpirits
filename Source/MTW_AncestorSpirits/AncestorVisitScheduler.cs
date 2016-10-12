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
        public float VisitorDays { get { return this.NumVisitors * this.DurationTicks / GenDate.TicksPerDay; } }

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

    public class VisitScheduleForSeason : IExposable
    {
        private List<VisitItinerary> itineraries = new List<VisitItinerary>();

        public float SeasonVisitorDays { get { return this.itineraries.Sum(i => i.VisitorDays); } }

        public VisitScheduleForSeason(List<VisitItinerary> itineraries)
        {
            this.itineraries = itineraries;
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.LookList<VisitItinerary>(ref this.itineraries, "itineraries", LookMode.Deep, new object[0]);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var itinerary in this.itineraries)
            {
                builder.AppendLine(itinerary.ToString());
            }

            return builder.ToString();
        }
    }

    public static class AncestorVisitScheduler
    {
        private static readonly IntRange numVisitorsRange = new IntRange(AncestorConstants.ANCESTORS_PER_VISIT, AncestorConstants.ANCESTORS_PER_VISIT);
        private static readonly IntRange numVisitsRange = new IntRange(2, 4);

        private static readonly IntRange visitDurationRangeTicks = new IntRange(AncestorUtils.DaysToTicks(1.75f),
            AncestorUtils.DaysToTicks(2.25f));
        private static readonly int bufferSeasonEnd = AncestorUtils.HoursToTicks(12f);
        private static readonly int bufferBetweenVisits = AncestorUtils.HoursToTicks(6f);

        private static readonly IntRange startTickRangeRel =
            new IntRange(0, GenDate.TicksPerSeason - visitDurationRangeTicks.max - bufferSeasonEnd);

        #region Estimation vars

        private static readonly float maxVisitorDaysPerSeason = numVisitorsRange.max * numVisitsRange.max *
            (visitDurationRangeTicks.max / GenDate.TicksPerDay);
        private static readonly float minVisitorDaysPerSeason = numVisitorsRange.min * numVisitsRange.min *
            (visitDurationRangeTicks.min / GenDate.TicksPerDay);
        private static readonly float avgVisitorDaysPerSeason = numVisitorsRange.Average * numVisitsRange.Average *
            (visitDurationRangeTicks.Average / GenDate.TicksPerDay);

        #endregion

        public static VisitScheduleForSeason BuildSeasonSchedule(long seasonStartTick)
        {
            int numVisits = numVisitsRange.RandomInRange;

            List<VisitItinerary> visitSchedule = new List<VisitItinerary>();
            int attempts = 0;
            while (visitSchedule.Count < numVisits && attempts < 500)
            {
                int numVisitors = numVisitorsRange.RandomInRange;
                int duration = visitDurationRangeTicks.RandomInRange;
                long start = seasonStartTick + startTickRangeRel.RandomInRange;

                VisitItinerary newItinerary = new VisitItinerary(numVisitors, start, duration);
                if (!visitSchedule.Any(i => i.Overlaps(newItinerary, bufferBetweenVisits)))
                {
                    visitSchedule.Add(newItinerary);
                }
                attempts++;
            }

            return new VisitScheduleForSeason(visitSchedule);
        }

        private static string EstApproval(float moodPercent)
        {
            List<float> MinMaxAvg = new List<float>();
            var approvalForInterval = ApprovalTracker.PawnApprovalForInterval(moodPercent);
            return String.Format("{0,6:###.##} | {1,6:###.##} | {2,6:###.##}",
                minVisitorDaysPerSeason * AncestorUtils.IntervalsPerDay * approvalForInterval,
                avgVisitorDaysPerSeason * AncestorUtils.IntervalsPerDay * approvalForInterval,
                maxVisitorDaysPerSeason * AncestorUtils.IntervalsPerDay * approvalForInterval);
        }

        public static string EstApprovalValues
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                var maxGainPerSeason = maxVisitorDaysPerSeason * ApprovalTracker.MaxGainPerDayPerAncestor;
                var maxLossPerSeason = maxVisitorDaysPerSeason * ApprovalTracker.MaxLossPerDayPerAncestor;

                builder.AppendFormat("Visitor min, avg, max days per season: ({0}, {1}, {2})",
                    minVisitorDaysPerSeason, avgVisitorDaysPerSeason, maxVisitorDaysPerSeason)
                    .AppendLine()
                    .AppendFormat("Max gain: {0}, max loss: {1}", maxGainPerSeason, maxLossPerSeason)
                    .AppendLine();

                foreach (var moodPercent in new List<float> { 1f, .9f, .8f, .7f, .6f, .5f, .4f, .3f, .2f, .1f, 0f })
                {
                    builder.AppendLine(EstApproval(moodPercent));
                }
                return builder.ToString();
            }
        }
    }
}
