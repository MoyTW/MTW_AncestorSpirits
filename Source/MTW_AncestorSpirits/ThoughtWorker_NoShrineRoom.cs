﻿using RimWorld;
using Verse;

namespace MTW_AncestorSpirits
{
    class ThoughtWorker_NoShrineRoom : ThoughtWorker
    {
        private static RoomRoleDef shrineRoomDef = DefDatabase<RoomRoleDef>.GetNamed("MTW_ShrineRoom");

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            // TODO: There's gotta be a better way of doin' this!
            if (!AncestorUtils.IsAncestor(p)) { return ThoughtState.Inactive; }

            var shrine = AncestorUtils.GetMapComponent().CurrentSpawner;
            if (shrine == null) { return ThoughtState.ActiveAtStage(1); }

            // HACK ALERT! Change Shrines to have an Interaction cell, and use that instead of a random one!
            var room = RoomQuery.RoomAtFast(shrine.RandomAdjacentCellCardinal());
            if (room == null)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (room.Role != shrineRoomDef)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else
            {
                return ThoughtState.Inactive;
            }
        }
    }
}
