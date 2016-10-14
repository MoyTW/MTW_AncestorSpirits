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
        private int numVisitors;
        private long startTickAbs;
        private int durationTicks;

        public bool HasFired;
        public int NumVisitors { get { return this.numVisitors; } }
        public long StartTickAbs { get { return this.startTickAbs; } }
        public int DurationTicks { get { return this.durationTicks; } }
        public long EndTickAbs { get { return this.StartTickAbs + this.DurationTicks; } }
        public float VisitorDays { get { return this.NumVisitors * this.DurationTicks / GenDate.TicksPerDay; } }

        // Required for IExposable
        public VisitItinerary() { }

        public VisitItinerary(int numVisitors, long startTickAbs, int durationTicks)
        {
            this.HasFired = false;
            this.numVisitors = numVisitors;
            this.startTickAbs = startTickAbs;
            this.durationTicks = durationTicks;
        }

        public bool Overlaps(VisitItinerary otherItinerary, int bufferBetweenVisits)
        {
            return this.StartTickAbs <= (otherItinerary.EndTickAbs + bufferBetweenVisits) &&
                (this.EndTickAbs + bufferBetweenVisits) >= otherItinerary.StartTickAbs;
        }

        public void FireVisit()
        {
            var visitConditionDef = DefDatabase<MapConditionDef>.GetNamed("MTW_AncestralVisit");
            MapCondition cond = MapConditionMaker.MakeCondition(visitConditionDef, this.DurationTicks, 0);
            Find.MapConditionManager.RegisterCondition(cond);

            this.HasFired = true;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.LookValue<bool>(ref this.HasFired, "HasFired");
            Scribe_Values.LookValue<int>(ref this.numVisitors, "NumVisitors");
            Scribe_Values.LookValue<long>(ref this.startTickAbs, "StartTickAbs");
            Scribe_Values.LookValue<int>(ref this.durationTicks, "DurationTicks");
        }

        public override string ToString()
        {
            return String.Format("{0} ancestors from {1} to {2} - fired={3}", this.NumVisitors,
                GenDate.DateReadoutStringAt((int)this.StartTickAbs), GenDate.DateReadoutStringAt((int)this.EndTickAbs),
                this.HasFired);
        }
    }

    public class VisitScheduleForSeason : IExposable
    {
        private Season seasonScheduledFor;
        private List<VisitItinerary> allItineraries = new List<VisitItinerary>();

        public Season SeasonScheduledFor { get { return this.seasonScheduledFor; } }
        public bool IsScheduledForCurrentSeason { get { return this.SeasonScheduledFor == GenDate.CurrentSeason; } }
        public VisitItinerary NextItinerary {
            get
            {
                return this.allItineraries.Where(i => !i.HasFired).FirstOrDefault();
            }
        }
        public float SeasonVisitorDays { get { return this.allItineraries.Sum(i => i.VisitorDays); } }

        public VisitScheduleForSeason() : this(Season.Undefined, new List<VisitItinerary>())
        { }

        public VisitScheduleForSeason(Season seasonScheduledFor, List<VisitItinerary> itineraries)
        {
            this.seasonScheduledFor = seasonScheduledFor;
            this.allItineraries = itineraries.OrderBy(i => i.StartTickAbs).ToList();
        }

        private void FireNextItinerary()
        {
            this.NextItinerary.FireVisit();
        }

        public void DisabledAlreadyPassedVisits()
        {
            long ticksAbs = Find.TickManager.TicksAbs;
            foreach (var itinerary in this.allItineraries)
            {
                if (itinerary.StartTickAbs < ticksAbs)
                    itinerary.HasFired = true;
            }
        }

        public void VisitScheduleTickInterval()
        {
            if (this.NextItinerary != null && Find.TickManager.TicksAbs > this.NextItinerary.StartTickAbs)
            {
                this.FireNextItinerary();
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.LookValue<Season>(ref this.seasonScheduledFor, "seasonScheduledFor");
            Scribe_Collections.LookList<VisitItinerary>(ref this.allItineraries, "allItineraries", LookMode.Deep, new object[0]);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var itinerary in this.allItineraries)
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

            return new VisitScheduleForSeason(GenDate.SeasonAt(seasonStartTick), visitSchedule);
        }

        public static VisitScheduleForSeason BuildSeasonScheduleForCurrentSeason()
        {
            long seasonStartTicks = AncestorUtils.EstStartOfSeasonAt(Find.TickManager.TicksAbs);
            return BuildSeasonSchedule(seasonStartTicks);
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
