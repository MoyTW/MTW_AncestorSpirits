using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    internal enum MemoType
    {
        positive = 0,
        negative,
        undecided
    }

    internal enum MemoCause
    {
        delta = 0,
        timer
    }

    // Does ScribeRef allow you to just scribe it as a...uh, ref?
    internal class AncestorMemo : IExposable
    {
        public static readonly IntRange PostOmenDelayRange =
            new IntRange(AncestorUtils.HoursToTicks(1), AncestorUtils.HoursToTicks(6));

        private int memoFireTick;
        private int omenFireTick;
        private MemoType type;
        private MemoCause cause;
        private bool finalized = false;
        private bool completed = false;

        public MemoCause Cause { get { return this.cause; } }
        public bool Finalized { get { return this.finalized; } }
        public bool Completed { get { return this.completed; } }

        public AncestorMemo() : this(AncestorMemoTimer.TicksBetween, MemoType.undecided, MemoCause.timer)
        {
        }

        public AncestorMemo(int ttl, MemoType type, MemoCause cause)
        {
            this.omenFireTick = Find.TickManager.TicksGame + ttl;

            this.memoFireTick = omenFireTick + PostOmenDelayRange.RandomInRange;
            this.type = type;
            this.cause = cause;
        }

        private void TryForceIncident(String name)
        {
            var incidentDef = DefDatabase<IncidentDef>.GetNamed(name);
            var incidentParams = new IncidentParms();
            incidentParams.forced = true;
            incidentDef.Worker.TryExecute(incidentParams);
        }

        private void FinalizeAndSendLetter(ApprovalTracker approval)
        {
            if (this.type == MemoType.undecided)
            {
                var hourHistoryDelta = approval.HourHistoryDelta();
                if (hourHistoryDelta >= 0.0)
                {
                    this.type = MemoType.positive;
                }
                else
                {
                    this.type = MemoType.negative;
                }
            }

            if (this.type == MemoType.positive)
            {
                Find.LetterStack.ReceiveLetter("Good Omen!", "One of your colonists has spotted a propitious omen! " +
                    " Surely the Ancestors are smiling on you.", LetterType.Good);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("Ill Omen!", "One of your colonists has spotted an inauspicious " +
                    "omen! Be wary in the coming hours.", LetterType.BadNonUrgent);
            }
            this.finalized = true;
        }

        private void FireAncestorMemo(ApprovalTracker approval)
        {
            AncestorMemoDef memoDef;
            DefDatabase<AncestorMemoDef>.AllDefsListForReading.Where(d => d.memoType == this.type).TryRandomElement(out memoDef);

            var incidentParams = new IncidentParms();
            incidentParams.forced = true;
            memoDef.Worker.TryExecute(incidentParams);
        }

        public void AncestorMemoTickInterval(ApprovalTracker approval)
        {
            if (!this.Finalized && this.omenFireTick <= Find.TickManager.TicksGame)
            {
                this.FinalizeAndSendLetter(approval);
            }
            else if (this.memoFireTick <= Find.TickManager.TicksGame)
            {
                this.FireAncestorMemo(approval);
                this.completed = true;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.memoFireTick, "ttl", AncestorMemoTimer.TicksBetween);
            Scribe_Values.LookValue<int>(ref this.omenFireTick, "omenTicks", PostOmenDelayRange.min);
            Scribe_Values.LookValue<MemoType>(ref this.type, "type", MemoType.undecided);
            Scribe_Values.LookValue<MemoCause>(ref this.cause, "cause", MemoCause.timer);
            Scribe_Values.LookValue<bool>(ref this.finalized, "finalized", false);
            Scribe_Values.LookValue<bool>(ref this.completed, "completed", false);
        }
    }


    class AncestorMemoTimer : IExposable
    {
        #region Vars & Accessors

        public static readonly int TicksBeforeFirst = AncestorUtils.DaysToTicks(7);
        public static readonly int TicksBetween = AncestorUtils.DaysToTicks(7);
        public static readonly int TicksPlusMinus = AncestorUtils.HoursToTicks(48);
        // These values are mostly arbitrary! Currently they're 2x the expected gain/loss per day per ancestor.
        public static readonly float TriggerPositiveMemoThreshold = AncestorUtils.SeasonValueToIntervalValue(1.0f);
        public static readonly double TriggerNegativeMemoThreshold = AncestorUtils.SeasonValueToIntervalValue(-2.0f);

        private AncestorMemo nextEvent = null;
        private AncestorMemo prevEvent = null;
        private Random _random = null;


        private Random Random
        {
            get
            {
                if (this._random == null)
                {
                    this._random = new Random();
                }
                return this._random;
            }
        }

        private bool CanScheduleDelta
        {
            get
            {
                return (!this.nextEvent.Finalized &&
                    this.nextEvent.Cause != MemoCause.delta &&
                    this.prevEvent != null &&
                    this.prevEvent.Cause != MemoCause.delta);
            }
        }

        #endregion

        public AncestorMemoTimer()
        {
            int timeToFirst = TicksBeforeFirst + this.GenTimerTicks();
            this.nextEvent = new AncestorMemo(timeToFirst, MemoType.undecided, MemoCause.timer);
        }

        #region Event Generation

        private int GenTimerTicks()
        {
            var multiplier = (this.Random.NextDouble() - 0.5) * 2;
            return (int)(multiplier * TicksPlusMinus) + TicksBetween;
        }

        private AncestorMemo GenTimerEvent()
        {
            return new AncestorMemo(this.GenTimerTicks(), MemoType.undecided, MemoCause.timer);
        }

        private AncestorMemo GenDeltaEvent(MemoType type)
        {
            // TODO: Passing in -1 here is kind of silly!
            return new AncestorMemo(-1, type, MemoCause.delta); // -1 causes it to finalize immediately
        }

        #endregion

        public void AncestorMemoTimerTickInterval(ApprovalTracker approval)
        {
            double intervalDelta = approval.IntervalDelta;
            this.nextEvent.AncestorMemoTickInterval(approval);

            if (this.nextEvent.Completed)
            {
                this.prevEvent = this.nextEvent;
                this.nextEvent = this.GenTimerEvent();
            }
            else if (this.nextEvent.Finalized)
            {
                return;
            }
            else if (approval.IntervalDelta > TriggerPositiveMemoThreshold &&
                this.CanScheduleDelta)
            {
                this.nextEvent = this.GenDeltaEvent(MemoType.positive);
            }
            else if (approval.IntervalDelta < TriggerNegativeMemoThreshold &&
                this.CanScheduleDelta)
            {
                this.nextEvent = this.GenDeltaEvent(MemoType.negative);
            }
        }

        public void ExposeData()
        {
            Scribe_Deep.LookDeep<AncestorMemo>(ref this.nextEvent, "nextEvent", new object[0]);
            Scribe_Deep.LookDeep<AncestorMemo>(ref this.prevEvent, "prevEvent", new object[0]);
        }
    }
}
