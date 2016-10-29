using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace MTW_AncestorSpirits
{
    class JobDriver_ReturnAnchor : JobDriver
    {
        private const int returnDurationTicks = 250;

        private const TargetIndex ShrineIndex = TargetIndex.A;
        private int ticksElapsed;

        public JobDriver_ReturnAnchor() : base()
        {
            this.ticksElapsed = 0;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Set fail conditions
            this.FailOnDestroyedNullOrForbidden(ShrineIndex);

            yield return Toils_Goto.GotoThing(ShrineIndex, PathEndMode.ClosestTouch);

            Toil toilReturn = new Toil();

            toilReturn.initAction = () =>
            {
                this.ticksElapsed = 0;
            };

            // We don't do it in the "ending" action because it'll cause a NPE - when you despawn the pawn it finishes
            // all the toils it has, and by then the pawn already has been despawned, so this.pawn is null.
            //
            // There's probably a more elegant way of fixing this.
            toilReturn.AddPreTickAction(() =>
            {
                if (this.ticksElapsed > returnDurationTicks)
                {
                    var ticker = Find.Map.GetComponent<MapComponent_AncestorTicker>();
                    ticker.Notify_ShouldDespawn(this.pawn, AncestorLeftCondition.LeftVoluntarily);
                    ReadyForNextToil();
                }
            });

            toilReturn.tickAction = () =>
            {
                this.ticksElapsed++;
            };

            toilReturn.defaultCompleteMode = ToilCompleteMode.Never;

            yield return toilReturn;
        }
    }
}
