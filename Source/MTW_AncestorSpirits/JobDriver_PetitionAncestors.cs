using RimWorld;
using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class JobDriver_PetitionAncestors : JobDriver
    {
        private const TargetIndex ShrineIndex = TargetIndex.A;
        // The driver holds this state? This is how it's done in the example, but...?
        private int ticksElapsed;

        public JobDriver_PetitionAncestors() : base()
        {
            ticksElapsed = 0;
        }

        private Job_PetitionAncestors Petition { get { return (Job_PetitionAncestors)this.CurJob; } }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set fail conditions
            this.FailOnDestroyedNullOrForbidden(ShrineIndex);

            yield return Toils_Reserve.Reserve(ShrineIndex);

            yield return Toils_Goto.GotoThing(ShrineIndex, PathEndMode.ClosestTouch);

            Toil toilPetition = new Toil();

            toilPetition.initAction = () =>
            {
                this.ticksElapsed = 0;
            };

            toilPetition.AddPreTickAction(() =>
            {
                if (this.ticksElapsed > this.Petition.PetitionTicks)
                    ReadyForNextToil();
            });

            toilPetition.tickAction = () =>
            {
                this.ticksElapsed++;
            };

            toilPetition.AddFinishAction(() =>
            {
                Log.Message("Petition sent! Name: " + this.Petition.PetitionName);
            });

            toilPetition.defaultCompleteMode = ToilCompleteMode.Never;

            yield return toilPetition;
        }
    }
}
