using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int ttl;
        private int omenTicks;
        private EventType type;
        private EventCause cause;
        private bool finalized = false;
        private bool completed = false;

        public EventCause Cause { get { return this.cause; } }
        public bool Finalized { get { return this.finalized; } }
        public bool Completed { get { return this.completed; } }

        public Event() : this((int)AncestorConstants.EVENT_TIMER_TICKS_BETWEEN, EventType.undecided, EventCause.timer)
        {
        }

        public Event(int ttl, EventType type, EventCause cause)
        {
            var rand = new Random(); // I mean, not great, but we're not gonna generate Events very much.
            this.omenTicks = rand.Next(AncestorConstants.EVENT_TIMER_MIN_OMEN_TICKS,
                AncestorConstants.EVENT_TIMER_MAX_OMEN_TICKS);

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

        private void FinalizeAndSendLetter(AncestorApproval approval)
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

        private void FireEvent(AncestorApproval approval)
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

        public void UpdateEvent(AncestorApproval approval)
        {
            this.ttl -= AncestorConstants.TICKS_PER_INTERVAL;

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
            Scribe_Values.LookValue<int>(ref this.ttl, "ttl", (int)AncestorConstants.EVENT_TIMER_TICKS_BETWEEN);
            Scribe_Values.LookValue<int>(ref this.omenTicks, "omenTicks", AncestorConstants.EVENT_TIMER_MIN_OMEN_TICKS);
            Scribe_Values.LookValue<EventType>(ref this.type, "type", EventType.undecided);
            Scribe_Values.LookValue<EventCause>(ref this.cause, "cause", EventCause.timer);
            Scribe_Values.LookValue<bool>(ref this.finalized, "finalized", false);
            Scribe_Values.LookValue<bool>(ref this.completed, "completed", false);
        }
    }


    class EventTimer : IExposable
    {
        #region Vars & Accessors

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

        public EventTimer()
        {
            int timeToFirst = (int)AncestorConstants.EVENT_TIMER_DAYS_BEFORE_FIRST + this.GenTimerTicks();
            this.nextEvent = new Event(timeToFirst, EventType.undecided, EventCause.timer);
        }

        #region Event Generation

        private int GenTimerTicks()
        {
            var multiplier = (this.Random.NextDouble() - 0.5) * 2;
            return (int)(multiplier * AncestorConstants.EVENT_TIMER_TICKS_PLUS_MINUS) + (int)AncestorConstants.EVENT_TIMER_TICKS_BETWEEN;
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

        public void UpdateTimer(AncestorApproval approval)
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
            else if (approval.IntervalDelta > AncestorConstants.EVENT_TRIGGER_GAIN_INTERVAL_DELTA &&
                this.CanScheduleDelta)
            {
                this.nextEvent = this.GenDeltaEvent(EventType.positive);
            }
            else if (approval.IntervalDelta < AncestorConstants.EVENT_TRIGGER_LOSS_INTERVAL_DELTA &&
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
