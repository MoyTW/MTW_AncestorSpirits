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

        public void UpdateTimer(AncestorApproval approval)
        {
            Log.Message("Running EventTimer");
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.ticksUntilNext, "ticksUntilNext",
                (int)(AncestorConstants.EVENT_TIMER_DAYS_BEFORE_FIRST * AncestorConstants.EVENT_TIMER_TICKS_BETWEEN));
        }
    }
}
