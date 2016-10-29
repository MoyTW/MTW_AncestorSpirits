using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class ThoughtWorker_ShrineRoomImpressiveness : ThoughtWorker
    {
        private static RoomRoleDef shrineRoomDef = DefDatabase<RoomRoleDef>.GetNamed("MTW_ShrineRoom");

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            // TODO: There's gotta be a better way of doin' this!
            if (!AncestorUtils.IsAncestor(p)) { return ThoughtState.Inactive; }

            var shrine = Find.Map.GetComponent<MapComponent_AncestorTicker>().CurrentSpawner;
            if (shrine == null) { return ThoughtState.Inactive; }

            // HACK ALERT! Change Shrines to have an Interaction cell, and use that instead of a random one!
            var room = RoomQuery.RoomAtFast(shrine.RandomAdjacentCellCardinal());
            if (room == null) { return ThoughtState.Inactive; }
            else if (room.Role != shrineRoomDef) { return ThoughtState.Inactive; }

            int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
            if (this.def.stages[scoreStageIndex] != null)
            {
                return ThoughtState.ActiveAtStage(scoreStageIndex);
            }
            return ThoughtState.Inactive;
        }
    }
}
