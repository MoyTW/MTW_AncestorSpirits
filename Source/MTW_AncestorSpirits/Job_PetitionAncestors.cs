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
        private string petitionName;

        public string PetitionName { get { return this.petitionName; } }

        public Job_PetitionAncestors() : base() { }
        public Job_PetitionAncestors(JobDef jdef, TargetInfo targetA, string petitionName) : base(jdef, targetA)
        {
            this.petitionName = petitionName;
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<string>(ref this.petitionName, "petitionName");
        }
    }
}
