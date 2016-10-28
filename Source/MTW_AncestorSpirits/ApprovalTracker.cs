using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class ApprovalTracker : IExposable
    {
        // Mood-based constants
        public const float PercentMoodForPositive = .4f;

        public const float MaxGainPerDayPerAncestor = .5f;
        public static readonly float PosIntervalMultiplier = AncestorUtils.DayValueToIntervalValue(MaxGainPerDayPerAncestor);

        public const float MaxLossPerDayPerAncestor = -1.0f;
        public static readonly float NegIntervalMultiplier = AncestorUtils.DayValueToIntervalValue(MaxLossPerDayPerAncestor);

        public const float MaxLossAnchorDestroyed = -4;
        public const float MaxLossAnchorBlocked = -3;

        // Shrine-based constants
        public static readonly int ShrineLossDelayTicks = AncestorUtils.DaysToTicks(7f);
        public static readonly float NoShrinesLossPerInterval = AncestorUtils.SeasonValueToIntervalValue(-8.0f);
        public static readonly float ManyShrinesLossPerInterval = AncestorUtils.SeasonValueToIntervalValue(-4.0f);

        private enum ShrineStatus
        {
            none = 0,
            one,
            many
        }

        private ShrineStatus previousShrineStatus = ShrineStatus.one;

        private double intervalDelta = 0;
        private double hourDeltaAcc = 0;
        private int hourOfDay;
        private int dayOfMonth;
        private Season season;
        private List<double> gaugeDeltaHour = new List<double>();
        private List<double> gaugeDeltaDay = new List<double>();

        // TODO: Store magic in own class!
        int currentMagic;

        public double IntervalDelta { get { return this.intervalDelta; } }
        public int CurrentMagic { get { return this.currentMagic; } }

        public double HourHistoryDelta()
        {
            return this.gaugeDeltaHour.Sum();
        }

        public void SubtractMagic(int magic)
        {
            this.currentMagic -= magic;
        }

        public ApprovalTracker()
        {
            this.currentMagic = AncestorConstants.MAGIC_START;
            this.hourOfDay = GenDate.HourOfDay;
            this.dayOfMonth = GenDate.DayOfMonth;
            this.season = GenDate.CurrentSeason;
        }

        public void SubmitVisitSummary(AncestralVisitSummary summary)
        {
            double sumApprovals = summary.visitHistories.Sum(p => p.ApprovalDelta);
            this.UpdateHistory(sumApprovals);
            if (sumApprovals > 0)
            {
                Messages.Message("Your Ancestors have concluded their visit. They were pleased.",
                    MessageSound.Negative);
            }
            else
            {
                Messages.Message("Your Ancestors have concluded their visit. They were not impressed.",
                    MessageSound.Negative);
            }
        }

        // TODO: Scale the approval loss depending on other factors?
        public static float PawnApprovalForAnchorDestruction()
        {
            return MaxLossAnchorDestroyed;
        }

        public static float PawnApprovalForAnchorBlocked()
        {
            return MaxLossAnchorBlocked;
        }

        public static float PawnApprovalForInterval(float moodPercent)
        {
            if (moodPercent > PercentMoodForPositive)
            {
                return ((moodPercent - PercentMoodForPositive) / (1 - PercentMoodForPositive)) * PosIntervalMultiplier;
            }
            else
            {
                return (1f - (moodPercent / PercentMoodForPositive)) * NegIntervalMultiplier;
            }
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
                return NoShrinesLossPerInterval;
            }
            else if (ancestorDriver.NumSpawners > 1)
            {
                if (this.previousShrineStatus != ShrineStatus.many)
                {
                    this.previousShrineStatus = ShrineStatus.many;
                    // TODO: Not hardcoded strings!
                    Find.LetterStack.ReceiveLetter("Too many Shrines!", "Your Ancestors are displeased that you have " +
                        "too many shrines! You should have one shrine, and one shrine only! You will lose approval with " +
                        "them until you demolish the extras.", LetterType.BadNonUrgent);
                }
                return ManyShrinesLossPerInterval;
            }
            else
            {
                this.previousShrineStatus = ShrineStatus.one;
                return 0;
            }
        }

        private void SummarizeSeason()
        {
            int lookBackDays = Math.Max(0, this.gaugeDeltaDay.Count - GenDate.DaysPerSeason);
            int deltaMagic = (int)this.gaugeDeltaDay.Skip(lookBackDays).Sum();

            this.currentMagic += deltaMagic;

            if (deltaMagic == 0)
            {
                // TODO: Hardcoded!
                Find.LetterStack.ReceiveLetter("Indifference",
                    "A new season is here!" +
                    Environment.NewLine + Environment.NewLine +
                    "Your Ancestors are unimpressed by your actions this past season." +
                    Environment.NewLine + Environment.NewLine +
                    "You have neither gained nor lost Magic.",
                    LetterType.BadNonUrgent);
            }
            else if (deltaMagic > 0)
            {
                // TODO: Hardcoded!
                Find.LetterStack.ReceiveLetter("Approval",
                    "A new season is here!" +
                    Environment.NewLine + Environment.NewLine +
                    "Your Ancestors are pleased by your actions this past season!" +
                    Environment.NewLine + Environment.NewLine +
                    "You have gained " + deltaMagic + " Magic!",
                    LetterType.Good);
            }
            else if (deltaMagic < 0)
            {
                // TODO: Hardcoded!
                Find.LetterStack.ReceiveLetter("Displeasure",
                    "A new season is here!" +
                    Environment.NewLine + Environment.NewLine +
                    "Your Ancestors are angered by your actions this past season!"
                    + Environment.NewLine + Environment.NewLine +
                    "You have lost " + Math.Abs(deltaMagic) + " Magic!",
                    LetterType.BadNonUrgent);
            }
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

        private void TryUpdateSeason()
        {
            Season season = GenDate.CurrentSeason;
            if (season != this.season)
            {
                this.season = season;
                this.SummarizeSeason();
            }
        }

        private void TryUpdateDay()
        {
            int dayOfMonth = GenDate.DayOfMonth;
            if (dayOfMonth != this.dayOfMonth)
            {
                this.dayOfMonth = dayOfMonth;
                this.SummarizeDay();
                this.TryUpdateSeason();
            }
        }

        private void TryUpdateHour()
        {
            int hourOfDay = GenDate.HourOfDay;
            if (hourOfDay != this.hourOfDay)
            {
                this.hourOfDay = hourOfDay;
                this.SummarizeHour();
                this.TryUpdateDay();
            }
        }

        private void UpdateHistory(double approvalDelta)
        {
            this.intervalDelta = approvalDelta;
            this.hourDeltaAcc += approvalDelta;

            this.TryUpdateHour();
        }

        public void ApprovalTrackerTickInterval(MapComponent_AncestorTicker ancestorDriver)
        {
            double approvalDelta = 0;

            if (Find.TickManager.TicksGame > ShrineLossDelayTicks)
            {
                approvalDelta += this.CalcShrineApproval(ancestorDriver);
            }

            this.UpdateHistory(approvalDelta);
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<ShrineStatus>(ref this.previousShrineStatus, "previousShrineStatus", ShrineStatus.one);
            Scribe_Values.LookValue<double>(ref this.intervalDelta, "intervalDelta", 0.0);
            Scribe_Values.LookValue<double>(ref this.hourDeltaAcc, "hourDeltaAcc", 0.0);
            Scribe_Values.LookValue<int>(ref this.hourOfDay, "hourOfDay", GenDate.HourOfDay);
            Scribe_Values.LookValue<int>(ref this.dayOfMonth, "dayOfMonth", GenDate.DayOfMonth);
            Scribe_Values.LookValue<Season>(ref this.season, "season", GenDate.CurrentSeason);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaHour, "gaugeDeltaHour", LookMode.Value, new object[0]);
            Scribe_Collections.LookList<double>(ref this.gaugeDeltaDay, "gaugeDeltaDay", LookMode.Value, new object[0]);
            Scribe_Values.LookValue<int>(ref this.currentMagic, "currentMagic", 0);
        }
    }
}
