using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class PetitionDef : Def
    {
        public Type workerClass;

        public float petitionHours;

        public int magicCost;

        public int PetitionTicks
        {
            get
            {
                return (int)(this.petitionHours * GenDate.TicksPerHour);
            }
        }

        public string PetitionSummary
        {
            get
            {
                return this.LabelCap + " (" + this.magicCost + " magic)";
            }
        }
    }
}
