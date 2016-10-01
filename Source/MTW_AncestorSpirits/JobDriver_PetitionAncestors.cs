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

        private Job_PetitionAncestors PetitionJob { get { return (Job_PetitionAncestors)this.CurJob; } }
        private PetitionDef PetitionDef { get { return this.PetitionJob.PetitionDef; } }

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
                if (this.ticksElapsed > this.PetitionJob.PetitionTicks)
                    ReadyForNextToil();
            });

            toilPetition.tickAction = () =>
            {
                this.ticksElapsed++;
            };

            toilPetition.AddFinishAction(() =>
            {
                Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_PetitionMade(this.PetitionDef, this.pawn);
            });

            toilPetition.defaultCompleteMode = ToilCompleteMode.Never;

            yield return toilPetition;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksElapsed, "ticksElapsed", 0);
        }
    }
}
