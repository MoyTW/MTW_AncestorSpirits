using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MTW_AncestorSpirits
{
    class LordToil_ReturnAnchor : LordToil
    {
        public override bool AllowSatisfyLongNeeds { get { return false; } }

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                DutyDef returnAnchorDef = DefDatabase<DutyDef>.GetNamed("MTW_ReturnAnchor");
                pawn.mindState.duty = new PawnDuty(returnAnchorDef);
            }
        }
    }
}
