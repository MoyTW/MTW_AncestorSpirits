using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class LordJob_HauntColony : LordJob
    {
        private IntVec3 chillSpot;

        public LordJob_HauntColony()
        {
            // Required
        }

        public LordJob_HauntColony(IntVec3 chillSpot)
        {
            this.chillSpot = chillSpot;
        }

        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {
            base.Notify_PawnLost(p, condition);

            switch (condition)
            {
                case PawnLostCondition.Vanished:
                    break;
                /* Theoretically the only valid loss condition is that the mod itself despawns them, since they're
                 * intended to be invincible. Hence any condition other than Vanished should never happen.
                 * 
                 * Theoretically. We're not there yet.
                 */
                case PawnLostCondition.ExitedMap:
                case PawnLostCondition.ChangedFaction:
                case PawnLostCondition.Undefined:
                case PawnLostCondition.IncappedOrKilled:
                case PawnLostCondition.MadePrisoner:
                case PawnLostCondition.LeftVoluntarily:
                case PawnLostCondition.Drafted:
                    throw new ArgumentException("Ancestor pawns should not be lost due to " + condition);
                default:
                    throw new ArgumentOutOfRangeException("condition");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref chillSpot, "chillSpot", default(IntVec3));
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graph = new StateGraph();
            LordToil_DefendPoint defendPoint = new LordToil_DefendPoint(this.chillSpot);
            graph.StartingToil = defendPoint;

            return graph;
        }
    }
}
