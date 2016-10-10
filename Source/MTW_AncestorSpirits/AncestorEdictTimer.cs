using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    internal enum EventType
    {
        positive = 0,
        negative,
        undecided
    }

    internal enum EventCause
    {
        delta = 0,
        timer
    }

    // Does ScribeRef allow you to just scribe it as a...uh, ref?
    internal class Event : IExposable
    {
        public static readonly IntRange OmenTicksRange =
            new IntRange(AncestorUtils.HoursToTicks(1), AncestorUtils.HoursToTicks(6));

        private int ttl;
        private int omenTicks;
        private EventType type;
        private EventCause cause;
        private bool finalized = false;
        private bool completed = false;

        public EventCause Cause { get { return this.cause; } }
        public bool Finalized { get { return this.finalized; } }
        public bool Completed { get { return this.completed; } }

        public Event() : this(AncestorEdictTimer.TicksBetween, EventType.undecided, EventCause.timer)
        {
        }

        public Event(int ttl, EventType type, EventCause cause)
        {
            this.omenTicks = OmenTicksRange.RandomInRange;

            this.ttl = Math.Max(ttl, this.omenTicks);
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
            if (this.type == EventType.undecided)
            {
                var hourHistoryDelta = approval.HourHistoryDelta();
                if (hourHistoryDelta >= 0.0)
                {
                    this.type = EventType.positive;
                }
                else
                {
                    this.type = EventType.negative;
                }
            }

            if (this.type == EventType.positive)
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

        private void FireEvent(ApprovalTracker approval)
        {
            if (this.type == EventType.positive)
            {
                this.TryForceIncident("ResourcePodCrash");
            }
            else
            {
                this.TryForceIncident("Flashstorm");
            }
        }

        public void UpdateEvent(ApprovalTracker approval)
        {
            // TODO: Uh!? You always call it on interval, and only on interval?
            this.ttl -= AncestorUtils.TicksPerInterval;

            if (!this.Finalized && this.ttl < this.omenTicks)
            {
                this.FinalizeAndSendLetter(approval);
            }
            else if (this.ttl <= 0)
            {
                this.FireEvent(approval);
                this.completed = true;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.ttl, "ttl", AncestorEdictTimer.TicksBetween);
            Scribe_Values.LookValue<int>(ref this.omenTicks, "omenTicks", OmenTicksRange.min);
            Scribe_Values.LookValue<EventType>(ref this.type, "type", EventType.undecided);
            Scribe_Values.LookValue<EventCause>(ref this.cause, "cause", EventCause.timer);
            Scribe_Values.LookValue<bool>(ref this.finalized, "finalized", false);
            Scribe_Values.LookValue<bool>(ref this.completed, "completed", false);
        }
    }


    class AncestorEdictTimer : IExposable
    {
        #region Vars & Accessors

        public static readonly int TicksBeforeFirst = AncestorUtils.DaysToTicks(7);
        public static readonly int TicksBetween = AncestorUtils.DaysToTicks(7);
        public static readonly int TicksPlusMinus = AncestorUtils.HoursToTicks(48);
        public static readonly float TriggerPositiveEdictThreshold = AncestorUtils.SeasonValueToIntervalValue(5.0f);
        public static readonly double TriggerNegativeEdictThreshold = AncestorUtils.SeasonValueToIntervalValue(-6.0f);

        private Event nextEvent = null;
        private Event prevEvent = null;
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
                return (this.nextEvent.Cause != EventCause.delta &&
                    this.prevEvent != null &&
                    this.prevEvent.Cause != EventCause.delta);
            }
        }

        #endregion

        public AncestorEdictTimer()
        {
            int timeToFirst = TicksBeforeFirst + this.GenTimerTicks();
            this.nextEvent = new Event(timeToFirst, EventType.undecided, EventCause.timer);
        }

        #region Event Generation

        private int GenTimerTicks()
        {
            var multiplier = (this.Random.NextDouble() - 0.5) * 2;
            return (int)(multiplier * TicksPlusMinus) + TicksBetween;
        }

        private Event GenTimerEvent()
        {
            return new Event(this.GenTimerTicks(), EventType.undecided, EventCause.timer);
        }

        private Event GenDeltaEvent(EventType type)
        {
            // TODO: Passing in -1 here is kind of silly!
            return new Event(-1, type, EventCause.delta); // -1 causes it to finalize immediately
        }

        #endregion

        public void EdictTimerTickInterval(ApprovalTracker approval)
        {
            double intervalDelta = approval.IntervalDelta;
            this.nextEvent.UpdateEvent(approval);

            if (this.nextEvent.Completed)
            {
                this.prevEvent = this.nextEvent;
                this.nextEvent = this.GenTimerEvent();
            }
            else if (this.nextEvent.Finalized)
            {
                return;
            }
            else if (approval.IntervalDelta > TriggerPositiveEdictThreshold &&
                this.CanScheduleDelta)
            {
                this.nextEvent = this.GenDeltaEvent(EventType.positive);
            }
            else if (approval.IntervalDelta < TriggerNegativeEdictThreshold &&
                this.CanScheduleDelta)
            {
                this.nextEvent = this.GenDeltaEvent(EventType.negative);
            }
        }

        public void ExposeData()
        {
            Scribe_Deep.LookDeep<Event>(ref this.nextEvent, "nextEvent", new object[0]);
            Scribe_Deep.LookDeep<Event>(ref this.prevEvent, "prevEvent", new object[0]);
        }
    }
}
