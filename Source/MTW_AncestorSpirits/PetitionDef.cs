using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class PetitionDef : IncidentDef
    {
        public float petitionHours;

        public int magicCost;

        public int MinMagic
        {
            get
            {
                return (int)Math.Ceiling((float)this.magicCost * AncestorConstants.PETITION_SPEND_MIN_MULT);
            }
        }

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
