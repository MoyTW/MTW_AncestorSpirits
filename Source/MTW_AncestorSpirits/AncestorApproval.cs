using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTW_AncestorSpirits
{
    class AncestorApproval : IExposable
    {
        private enum ShrineStatus
        {
            none = 0,
            one,
            many
        }

        private ShrineStatus previousShrineStatus = ShrineStatus.one;

        private double intervalDelta = 0;
        private double hourDeltaAcc = 0;
        private List<double> gaugeDeltaHour = new List<double>();
        private List<double> gaugeDeltaDay = new List<double>();

        public double IntervalDelta
        {
            get
            {
                return this.intervalDelta;
            }
        }

        public double HourHistoryDelta()
        {
            return this.gaugeDeltaHour.Sum();
        }

        private double CalcShrineApproval(MapComponent_AncestorTicker ancestorDriver)
        {
            if (ancestorDriver.CurrentSpawner == null)
            {
                if (this.previousShrineStatus != ShrineStatus.none)
                {
                    this.previousShrineStatus = ShrineStatus.none;
                    // TODO: Not hardcoded strings!
                    Find.LetterStack.ReceiveLetter("No Shrines!", "Your Ancestors are displeased that you have no " +
                        "shrines! You will lose approval with them until you build a shrine.", LetterType.BadNonUrgent);
                }
                return AncestorConstants.APP_MOD_NO_SHRINES_INTERVAL;
            }
            else if (ancestorDriver.NumSpawners > 1)
            {
                if (this.previousShrineStatus != ShrineStatus.many)
                {
                    this.previousShrineStatus = ShrineStatus.many;
                    // TODO: Not hardcoded strings!
                    Find.LetterStack.ReceiveLetter("Too many Shrines!", "Your Ancestors are displeased that you have " +
                        "too many shrines! You should have one shrine, and one shrine only! You will lose approval with " +
                        " them until you demolish the extras.", LetterType.BadNonUrgent);
                }
                return AncestorConstants.APP_MOD_MANY_SHRINES_INTERVAL;
            }
            else
            {
                this.previousShrineStatus = ShrineStatus.one;
                return 0;
            }
        }

        private double CalcPawnApproval(MapComponent_AncestorTicker ancestorDriver)
        {
            var visitingPawns = ancestorDriver.AncestorsVisiting;
            double approvalSum = 0;

            foreach (Pawn p in visitingPawns)
            {
                double cutMoodPercent = p.needs.mood.CurInstantLevelPercentage - AncestorConstants.APP_NEG_CUTOFF;
                if (cutMoodPercent > 0)
                {
                    approvalSum += cutMoodPercent * AncestorConstants.APP_MULT_GAIN_PER_SEASON;
                }
                else
                {
                    approvalSum += cutMoodPercent * AncestorConstants.APP_MULT_LOSS_PER_SEASON;
                }
            }
            return approvalSum;
        }

        private void SummarizeHour()
        {
            if (this.gaugeDeltaHour.Count < AncestorConstants.APPROVAL_HISTORY_HOURS)
            {
                this.gaugeDeltaHour.Add(this.hourDeltaAcc);
            }
            else
            {
                this.gaugeDeltaHour.RemoveAt(0);
                this.gaugeDeltaHour.Add(this.hourDeltaAcc);
            }
            this.hourDeltaAcc = 0.0;
        }

        private void SummarizeDay()
        {
            var deltaLastDay = this.gaugeDeltaHour.Skip(this.gaugeDeltaHour.Count - 24).Sum();
            this.gaugeDeltaDay.Add(deltaLastDay);
        }

        private void UpdateHistory(double approvalDelta)
        {
            this.intervalDelta = approvalDelta;
            this.hourDeltaAcc += approvalDelta;

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                this.SummarizeHour();
                var sh = "";
                foreach (var d in this.gaugeDeltaHour)
                {
                    var sadd = ", " + d;
                    sh += sadd;
                }
                Log.Message("Hour summary: " + sh);

                if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
                {
                    this.SummarizeDay();
                    var sd = "";
                    foreach (var d in this.gaugeDeltaDay)
                    {
                        var sadd = ", " + d;
                        sd += sadd;
                    }
                    Log.Message("Day summary: " + sd);
                }
            }
        }

        public void UpdateApproval(MapComponent_AncestorTicker ancestorDriver)
        {
            double approvalDelta = 0;
            approvalDelta += this.CalcPawnApproval(ancestorDriver);
            approvalDelta += this.CalcShrineApproval(ancestorDriver);

            this.UpdateHistory(approvalDelta);

            Log.Message("Approval: " + this.hourDeltaAcc.ToString());
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<ShrineStatus>(ref this.previousShrineStatus, "previousShrineStatus", ShrineStatus.one);
            Scribe_Values.LookValue<double>(ref this.intervalDelta, "intervalDelta", 0.0);
            Scribe_Values.LookValue<double>(ref this.hourDeltaAcc, "hourDeltaAcc", 0.0);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaHour, "gaugeDeltaHour", LookMode.Value, new object[0]);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaDay, "gaugeDeltaDay", LookMode.Value, new object[0]);
        }
    }
}
