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
        private int ticksUntilNext = 0;
        private bool canFireDelta = true;
        private Random _random = null;

        private enum EventType
        {
            positive = 0,
            negative
        }

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

        private int TicksToNextEvent()
        {
            var multiplier = (this.Random.NextDouble() - 0.5) * 2;
            return (int)(multiplier * AncestorConstants.EVENT_TIMER_TICKS_PLUS_MINUS);
        }

        private void ScheduleNextEvent()
        {
            this.ticksUntilNext = this.TicksToNextEvent();
        }

        public EventTimer()
        {
            this.ticksUntilNext = (int)AncestorConstants.EVENT_TIMER_DAYS_BEFORE_FIRST + this.TicksToNextEvent();
        }

        private void TryForceIncident(String name)
        {
            var incidentDef = DefDatabase<IncidentDef>.GetNamed(name);
            var incidentParams = new IncidentParms();
            incidentParams.forced = true;
            incidentDef.Worker.TryExecute(incidentParams);
        }

        private void FireEvent(EventType type)
        {
            if (type == EventType.positive)
            {
                this.TryForceIncident("ResourcePodCrash");
            }
            else
            {
                this.TryForceIncident("Flashstorm");
            }
            this.ScheduleNextEvent();
        }

        public void UpdateTimer(AncestorApproval approval)
        {
            double intervalDelta = approval.IntervalDelta;
            this.ticksUntilNext -= AncestorConstants.TICK_INTERVAL;

            // Shouldn't trigger immediately - should schedule for next hour
            if (this.canFireDelta && approval.IntervalDelta > AncestorConstants.EVENT_TRIGGER_GAIN_INTERVAL_DELTA)
            {
                Log.Message("Immediate positive event!");
                this.FireEvent(EventType.positive);
                this.canFireDelta = false;
            }
            else if (this.canFireDelta && approval.IntervalDelta < AncestorConstants.EVENT_TRIGGER_LOSS_INTERVAL_DELTA)
            {
                Log.Message("Immediate negative event!");
                this.FireEvent(EventType.negative);
                this.canFireDelta = false;
            }
            else if (this.ticksUntilNext <= 0)
            {
                var hourHistoryDelta = approval.HourHistoryDelta();
                Log.Message("Triggering event - delta = " + hourHistoryDelta);
                if (hourHistoryDelta >= 0.0)
                {
                    this.FireEvent(EventType.positive);
                }
                else
                {
                    this.FireEvent(EventType.negative);
                }
                this.canFireDelta = true;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.ticksUntilNext, "ticksUntilNext",
                (int)(AncestorConstants.EVENT_TIMER_DAYS_BEFORE_FIRST * AncestorConstants.EVENT_TIMER_TICKS_BETWEEN));
            Scribe_Values.LookValue<bool>(ref this.canFireDelta, "canFireDelta", true);
        }
    }
}
