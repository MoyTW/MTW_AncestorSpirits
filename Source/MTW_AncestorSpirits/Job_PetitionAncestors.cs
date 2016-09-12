using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class Job_PetitionAncestors : Job
    {
        private PetitionDef petitionDef;

        public string PetitionName { get { return this.petitionDef.LabelCap; } }
        public int PetitionTicks { get { return this.petitionDef.PetitionTicks; } }

        public Job_PetitionAncestors() : base() { }
        public Job_PetitionAncestors(JobDef jdef, TargetInfo targetA, PetitionDef petitionDef) : base(jdef, targetA)
        {
            this.petitionDef = petitionDef;
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef<PetitionDef>(ref this.petitionDef, "petitionDef");
        }
    }
}
