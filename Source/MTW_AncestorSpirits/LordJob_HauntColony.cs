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
        private int duration;

        public LordJob_HauntColony()
        {
            // Required
        }

        public LordJob_HauntColony(IntVec3 chillSpot, int duration)
        {
            this.chillSpot = chillSpot;
            this.duration = duration;
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
            Scribe_Values.LookValue(ref this.duration, "duration");
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graph = new StateGraph();
            var hauntPointToil = new LordToil_Relax();
            graph.StartingToil = hauntPointToil;

            LordToil returnAnchorToil = new LordToil_ReturnAnchor();
            graph.lordToils.Add(returnAnchorToil);
            Transition t1 = new Transition(hauntPointToil, returnAnchorToil);
            t1.triggers.Add(new Trigger_TicksPassed(this.duration));
            t1.preActions.Add(new TransitionAction_Message("Your Ancestors are leaving!"));
            graph.transitions.Add(t1);

            return graph;
            /* wowee thar's a doozy
            StateGraph graphArrive = new StateGraph();
            var travelGraph = new LordJob_Travel(chillSpot).CreateGraph();
            travelGraph.StartingToil = new LordToil_CustomTravel(chillSpot, 0.49f); // CHANGED: override StartingToil
            LordToil toilArrive = graphArrive.AttachSubgraph(travelGraph).StartingToil;
            var toilVisit = new LordToil_VisitPoint(chillSpot); // CHANGED
            graphArrive.lordToils.Add(toilVisit);
            LordToil toilTakeWounded = new LordToil_TakeWoundedGuest();
            graphArrive.lordToils.Add(toilTakeWounded);
            StateGraph graphExit = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
            LordToil toilExit = graphArrive.AttachSubgraph(graphExit).StartingToil;
            LordToil toilLeaveMap = graphExit.lordToils[1];
            LordToil toilLost = new LordToil_End();
            graphExit.AddToil(toilLost);
            Transition t1 = new Transition(toilArrive, toilVisit);
            t1.triggers.Add(new Trigger_Memo("TravelArrived"));
            graphArrive.transitions.Add(t1);
            LordToil_ExitMapBest toilExitCold = new LordToil_ExitMapBest(); // ADDED TOIL
            graphArrive.AddToil(toilExitCold);
            Transition t6 = new Transition(toilArrive, toilExitCold); // ADDED TRANSITION
            t6.triggers.Add(new Trigger_UrgentlyCold());
            t6.preActions.Add(new TransitionAction_Message("MessageVisitorsLeavingCold".Translate(new object[] { faction.Name })));
            t6.preActions.Add(new TransitionAction_Custom(() => StopPawns(lord.ownedPawns)));
            graphArrive.transitions.Add(t6);
            Transition t2 = new Transition(toilVisit, toilTakeWounded);
            t2.triggers.Add(new Trigger_WoundedGuestPresent());
            t2.preActions.Add(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(new object[] { faction.def.pawnsPlural.CapitalizeFirst(), faction.Name })));
            graphArrive.transitions.Add(t2);
            Transition t3 = new Transition(toilVisit, toilLeaveMap);
            t3.triggers.Add(new Trigger_BecameColonyEnemy());
            t3.preActions.Add(new TransitionAction_WakeAll());
            t3.preActions.Add(new TransitionAction_SetDefendLocalGroup());
            graphArrive.transitions.Add(t3);
            Transition t4 = new Transition(toilArrive, toilExit);
            t4.triggers.Add(new Trigger_BecameColonyEnemy());
            //t4.triggers.Add(new Trigger_VisitorsPleasedMax(MaxPleaseAmount(faction.ColonyGoodwill))); // CHANGED
            t4.triggers.Add(new Trigger_VisitorsAngeredMax(IncidentWorker_VisitorGroup.MaxAngerAmount(faction.PlayerGoodwill))); // CHANGED
            t4.preActions.Add(new TransitionAction_WakeAll());
            t4.preActions.Add(new TransitionAction_EnsureHaveExitDestination());
            graphArrive.transitions.Add(t4);
            Transition t5 = new Transition(toilVisit, toilExit);
            t5.triggers.Add(new Trigger_TicksPassed(Rand.Range(16000, 44000)));
            t5.preActions.Add(new TransitionAction_Message("VisitorsLeaving".Translate(new object[] { faction.Name })));
            t5.preActions.Add(new TransitionAction_WakeAll());
            t5.preActions.Add(new TransitionAction_EnsureHaveExitDestination());
            graphArrive.transitions.Add(t5);
            return graphArrive;
            */
        }
    }
}
