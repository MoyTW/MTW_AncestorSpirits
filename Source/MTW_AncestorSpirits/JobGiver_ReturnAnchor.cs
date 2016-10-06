using RimWorld;
using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var spawner = Find.Map.GetComponent<MapComponent_AncestorTicker>().CurrentSpawner;
            if (spawner == null) { return null; }

            IntVec3 anchorCell;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(spawner, out anchorCell);
            if (anchorCell == null) { return null; }

            Find.PawnDestinationManager.ReserveDestinationFor(pawn, anchorCell);
            return new Job(DefDatabase<JobDef>.GetNamed("MTW_ReturnAnchor"), spawner);
        }
    }
}
