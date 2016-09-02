﻿using RimWorld;
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
        private List<double> gaugeDeltaHour = new List<double>();
        private List<double> gaugeDeltaDay = new List<double>();

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

            // Pawn approval summary
            foreach (Pawn p in visitingPawns)
            {
                double moodPercent = p.needs.mood.CurInstantLevelPercentage;
                approvalSum += (moodPercent / 100) * AncestorConstants.APPROVAL_INTERVAL_MULTIPLIER;
            }
            return approvalSum;
        }

        private void SummarizeHour()
        {
            if (this.gaugeDeltaHour.Count < AncestorConstants.APPROVAL_HISTORY_HOURS)
            {
                this.gaugeDeltaHour.Add(this.intervalDelta);
            }
            else
            {
                this.gaugeDeltaHour.RemoveAt(0);
                this.gaugeDeltaHour.Add(this.intervalDelta);
            }
            this.intervalDelta = 0.0;
        }

        private void SummarizeDay()
        {
            var gagueLastHour = this.gaugeDeltaHour.Skip(this.gaugeDeltaHour.Count - 24);
            this.gaugeDeltaDay.Add(gagueLastHour.Sum());
        }

        private void UpdateHistory(double approvalDelta)
        {
            this.intervalDelta += approvalDelta;

            if (Find.TickManager.TicksGame % AncestorConstants.TICKS_PER_HOUR == 0)
            {
                this.SummarizeHour();
                var sh = "";
                foreach (var d in this.gaugeDeltaHour)
                {
                    var sadd = ", " + d;
                    sh += sadd;
                }
                Log.Message("Hour summary: " + sh);

                if (Find.TickManager.TicksGame % AncestorConstants.TICKS_PER_DAY == 0)
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

            Log.Message("Approval: " + this.intervalDelta.ToString());
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<ShrineStatus>(ref this.previousShrineStatus, "previousShrineStatus", ShrineStatus.one);
            Scribe_Values.LookValue<double>(ref this.intervalDelta, "intervalDelta", 0.0);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaHour, "gaugeDeltaHour", LookMode.Value, new object[0]);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaDay, "gaugeDeltaDay", LookMode.Value, new object[0]);
        }
    }
}
