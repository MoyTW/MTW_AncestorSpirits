using RimWorld;
using Verse;
using Verse.AI;

namespace MTW_AncestorSpirits
{
    class JobGiver_InspectOutsideOfColony : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return Rand.Range(.95f, 1.0f);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJob != null) { return pawn.CurJob; }

            IntVec3 justOutsideColony;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out justOutsideColony);

            if (justOutsideColony != null && Reachability.CanReach(pawn, justOutsideColony, PathEndMode.OnCell, Danger.Deadly))
            {
                return new Job(JobDefOf.GotoWander, justOutsideColony);
            }
            else
            {
                // TODO: Ancestors angered if they cannot go outside
                return null;
            }
        }
    }
}
