using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTW_AncestorSpirits
{
    class EventTimer : IExposable
    {
        #region Data Types

        private enum EventType
        {
            positive = 0,
            negative,
            undecided
        }

        private enum EventCause
        {
            delta = 0,
            timer
        }

        // Does ScribeRef allow you to just scribe it as a...uh, ref?
        private class Event : IExposable
        {
            public int ttl;
            public EventType type;
            public EventCause cause;

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
            }
        }

        #endregion

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
                return (this.nextEvent.cause != EventCause.delta &&
                    this.prevEvent != null &&
                    this.prevEvent.cause != EventCause.delta);
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

        private void TryForceIncident(String name)
        {
            var incidentDef = DefDatabase<IncidentDef>.GetNamed(name);
            var incidentParams = new IncidentParms();
            incidentParams.forced = true;
            incidentDef.Worker.TryExecute(incidentParams);
        }

        private void FireNextEvent(AncestorApproval approval)
        {
            // If undecided, resolve immediately
            if (this.nextEvent.type == EventType.undecided)
            {
                var hourHistoryDelta = approval.HourHistoryDelta();
                if (hourHistoryDelta >= 0.0)
                {
                    this.nextEvent.type = EventType.positive;
                }
                else
                {
                    this.nextEvent.type = EventType.negative;
                }
            }

            if (this.nextEvent.type == EventType.positive)
            {
                this.TryForceIncident("ResourcePodCrash");
            }
            else
            {
                this.TryForceIncident("Flashstorm");
            }

            this.prevEvent = this.nextEvent;
            this.nextEvent = this.GenTimerEvent();
        }

        public void UpdateTimer(AncestorApproval approval)
        {
            double intervalDelta = approval.IntervalDelta;
            this.nextEvent.ttl -= AncestorConstants.TICK_INTERVAL;

            if (this.nextEvent.ttl <= 0)
            {
                this.FireNextEvent(approval);
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
