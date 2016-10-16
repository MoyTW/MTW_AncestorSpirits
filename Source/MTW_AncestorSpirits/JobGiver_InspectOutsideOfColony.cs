using RimWorld;
using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                Log.Message("Inspecting outside at: " + justOutsideColony.ToString());
                return new Job(JobDefOf.GotoWander, justOutsideColony);
            }
            else
            {
                Log.Message("Cannot go outside! This should cause you Ancestors to be angered!");
                return null;
            }
        }
    }
}
