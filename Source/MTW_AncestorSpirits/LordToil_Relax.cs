using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MTW_AncestorSpirits
{
    class LordToil_Relax : LordToil
    {
        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                DutyDef relaxDef = DefDatabase<DutyDef>.GetNamed("MTW_Relax");
                pawn.mindState.duty = new PawnDuty(relaxDef);
            }
        }
    }
}
