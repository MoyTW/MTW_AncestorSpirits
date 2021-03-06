﻿using RimWorld;

using System;

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

        public float SuccesChance(int magicUsed)
        {
            if (magicUsed < this.MinMagic)
            {
                return 0.0f;
            }
            else
            {
                float percentUsedOverMin = ((float)magicUsed - (float)this.MinMagic) / (float)this.magicCost;
                return AncestorConstants.PETITION_BASE_SUCCESS +
                    (1.0f - AncestorConstants.PETITION_BASE_SUCCESS) * percentUsedOverMin;
            }
        }

        public int PetitionTicks
        {
            get
            {
                return AncestorUtils.HoursToTicks(this.petitionHours);
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
