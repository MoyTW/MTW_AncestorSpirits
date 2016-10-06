using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
