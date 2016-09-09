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
        private EventType type;
        private EventCause cause;
        private bool sentOmen = false;
        private bool completed = false;

        public EventCause Cause { get { return this.cause; } }
        public bool SentOmen { get { return this.SentOmen; } } // TODO: Add in omens!
        public bool Finalized { get { return this.SentOmen; } }
        public bool Completed { get { return this.completed; } }

        private void TryForceIncident(String name)
        {
            var incidentDef = DefDatabase<IncidentDef>.GetNamed(name);
            var incidentParams = new IncidentParms();
            incidentParams.forced = true;
            incidentDef.Worker.TryExecute(incidentParams);
        }

        private void FireEvent(AncestorApproval approval)
        {
            // If undecided, resolve immediately
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
                this.TryForceIncident("ResourcePodCrash");
            }
            else
            {
                this.TryForceIncident("Flashstorm");
            }
        }

        public void UpdateEvent(AncestorApproval approval)
        {
            this.ttl -= AncestorConstants.TICK_INTERVAL;

            if (this.ttl <= 0)
            {
                this.FireEvent(approval);
                this.completed = true;
            }
        }

        public Event()
        {
            this.ttl = (int)AncestorConstants.EVENT_TIMER_TICKS_BETWEEN;
            this.type = EventType.undecided;
            this.cause = EventCause.timer;
        }

        public Event(int ttl, EventType type, EventCause cause)
        {
            this.ttl = ttl;
            this.type = type;
            this.cause = cause;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.ttl, "ttl", (int)AncestorConstants.EVENT_TIMER_TICKS_BETWEEN);
            Scribe_Values.LookValue<EventType>(ref this.type, "type", EventType.undecided);
            Scribe_Values.LookValue<EventCause>(ref this.cause, "cause", EventCause.timer);
            Scribe_Values.LookValue<bool>(ref this.sentOmen, "sentOmen", false);
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

        // Generates an event which will fire between 1 and 2 hours from generation time
        private int GenDeltaTicks()
        {
            int ticksInHour = (int)(this.Random.NextDouble() * (double)AncestorConstants.TICKS_PER_HOUR);
            return ticksInHour + AncestorConstants.TICKS_PER_HOUR;
        }

        private Event GenDeltaEvent(EventType type)
        {
            return new Event(this.GenDeltaTicks(), type, EventCause.delta);
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
