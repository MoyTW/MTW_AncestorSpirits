using RimWorld;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public static class AncestorUtils
    {
        public static int DaysToTicks(float days)
        {
            return Mathf.RoundToInt(days * GenDate.TicksPerDay);
        }

        public static int HoursToTicks(float hours)
        {
            return Mathf.RoundToInt(hours * GenDate.TicksPerHour);
        }
    }
}
