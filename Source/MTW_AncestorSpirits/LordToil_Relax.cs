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
    class LordToilData_HauntPoint : LordToilData
    {
        public IntVec3 point;
        public float radius;
        public Dictionary<int, float> visitorMoods = new Dictionary<int, float>();

        public override void ExposeData()
        {
            Scribe_Values.LookValue(ref point, "point", default(IntVec3));
            Scribe_Values.LookValue(ref radius, "radius", 45f);
            Scribe_Collections.LookDictionary(ref visitorMoods, "visitorMoods");
        }
    }

    class LordToil_Relax : LordToil
    {
        public LordToilData_HauntPoint Data { get { return (LordToilData_HauntPoint)this.data; } }
        public override IntVec3 FlagLoc
        {
            get { return this.Data.point; }
        }

        public LordToil_Relax(IntVec3 point)
        {
            this.data = new LordToilData_HauntPoint { point = point };
        }

        // looking
        public override void Init()
        {
            base.Init();
            //Log.Message("Init State_VisitPoint "+brain.ownedPawns.Count + " - "+brain.faction.name);
            foreach (var pawn in this.lord.ownedPawns)
            {
                //if (pawn.needs == null || pawn.needs.mood == null) Data.visitorMoods.Add(pawn.thingIDNumber, 0.5f);
                //else
                this.Data.visitorMoods.Add(pawn.thingIDNumber, pawn.needs.mood.CurLevel);
                //Log.Message("Added "+pawn.NameStringShort+": "+pawn.needs.mood.CurLevel);

                var newColony = -0.1f; // Mathf.Lerp(-0.15f, -0.05f, GenDate.MonthsPassed/20f); // bonus for new colony
                float expectations = newColony;
                Data.visitorMoods[pawn.thingIDNumber] += expectations;
            }
        }

        public override void Cleanup()
        {
            Leave();

            base.Cleanup();
        }

        private void Leave()
        {
            // If Ancestors visit for a limited period, this is where to sum up the visit & apply Approval change
            Log.Message("Leave not yet implemented!");
        }

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                DutyDef relaxDef = DefDatabase<DutyDef>.GetNamed("MTW_Relax");
                pawn.mindState.duty = new PawnDuty(relaxDef, FlagLoc, Data.radius);
            }
        }
    }
}
