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
        public override float GetPriority(Pawn pawn)
        {
            var anchorNeed = pawn.needs.TryGetNeed<Need_Anchor>();
            if (anchorNeed == null)
            {
                Log.Message(pawn.NameStringShort + " has no anchor need!");
                return 0f;
            }
            if (anchorNeed.IsLow)
            {
                return 10f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJob != null)
            {
                return pawn.CurJob;
            }
            if (pawn.needs.TryGetNeed<Need_Anchor>() == null)
            {
                Log.Message(pawn.NameStringShort + " has no anchor need! Cannot give ReturnAnchor job!");
                return null;
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
