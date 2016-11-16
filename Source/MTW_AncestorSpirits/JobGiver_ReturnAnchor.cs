using Verse;
using Verse.AI;

namespace MTW_AncestorSpirits
{
    class JobGiver_ReturnAnchor : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJob != null)
            {
                return pawn.CurJob;
            }

            var spawner = AncestorUtils.GetMapComponent().CurrentSpawner;
            if (spawner == null) { return null; }

            IntVec3 anchorCell;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(spawner, out anchorCell);
            if (anchorCell == null) { return null; }

            var path = PathFinder.FindPath(pawn.Position, anchorCell,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything));
            IntVec3 cellBeforeBlocker;
            Thing blocker = path.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
            if (blocker != null)
            {
                var ticker = AncestorUtils.GetMapComponent();
                ticker.Notify_ShouldDespawn(pawn, AncestorLeftCondition.AnchorBlocked);
            }
            path.Dispose();

            Find.PawnDestinationManager.ReserveDestinationFor(pawn, anchorCell);
            return new Job(DefDatabase<JobDef>.GetNamed("MTW_ReturnAnchor"), spawner);
        }
    }
}
