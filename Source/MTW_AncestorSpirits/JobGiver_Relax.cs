﻿using RimWorld;
using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    // Copied from Hospitality - MTW
    public class JobGiver_Relax : ThinkNode_JobGiver
    {
        DefMap<JoyGiverDef, float> joyGiverChances;

        public override float GetPriority(Pawn pawn)
        {
            if (pawn.needs == null || pawn.needs.joy == null)
            {
                Log.ErrorOnce(pawn.NameStringShort + " needs no joy...", 7392 + pawn.thingIDNumber);
                return 0f;
            }
            float curLevel = pawn.needs.joy.CurLevel;

            if (curLevel < 0.35f)
            {
                return 6f;
            }
            return 1 - curLevel;
        }

        public override void ResolveReferences()
        {
            joyGiverChances = new DefMap<JoyGiverDef, float>();
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJob != null)
            {
                //Log.ErrorOnce(pawn.NameStringShort+ " already has a job: "+pawn.CurJob, 4325+pawn.thingIDNumber);
                return pawn.CurJob;
            }
            if (pawn.needs == null) Log.ErrorOnce(pawn.NameStringShort + " has no needs", 3463 + pawn.thingIDNumber);
            if (pawn.needs.joy == null) Log.ErrorOnce(pawn.NameStringShort + " has no joy need", 8585 + pawn.thingIDNumber);
            if (pawn.skills == null) Log.ErrorOnce(pawn.NameStringShort + " has no skills", 22352 + pawn.thingIDNumber);
            if (pawn.GetTimeAssignment() == null) Log.ErrorOnce(pawn.NameStringShort + " has no time assignments", 74564 + pawn.thingIDNumber);
            var allDefsListForReading = PopulateChances(pawn);
            for (int j = 0; j < joyGiverChances.Count; j++)
            {
                JoyGiverDef giverDef;
                if (!allDefsListForReading.TryRandomElementByWeight(d => joyGiverChances[d], out giverDef))
                {
                    //Log.ErrorOnce(pawn.NameStringShort + " has no random element.",45784 +pawn.thingIDNumber);
                    return null;
                }
                var job = giverDef.Worker.TryGiveJob(pawn);
                if (job != null)
                {
                    return job;
                }
                joyGiverChances[giverDef] = 0f;
            }
            Log.ErrorOnce(pawn.NameStringShort + " did not get a relax job.", 45745 + pawn.thingIDNumber);
            return null;
        }

        private List<JoyGiverDef> PopulateChances(Pawn pawn)
        {
            var chemicalDef = DefDatabase<JoyKindDef>.GetNamed("Chemical");
            var gluttonousDef = DefDatabase<JoyKindDef>.GetNamed("Gluttonous");

            List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
            if (allDefsListForReading == null)
            {
                Log.ErrorOnce("AllDefsListForReading == null", 331093564);
                return new List<JoyGiverDef>();
            }
            int i = 0;
            while (i < allDefsListForReading.Count)
            {
                // What is going on here with the jumps!? Is this a decompiler artifact?
                JoyGiverDef joyGiverDef = allDefsListForReading[i];

                // Exclude consumption-based joys (Spirits cannot now eat!)
                if (joyGiverDef.joyKind == chemicalDef || joyGiverDef.joyKind == gluttonousDef)
                {
                    goto IL_A5;
                }

                if (joyGiverDef.pctPawnsEverDo >= 1f)
                {
                    goto IL_6B;
                }
                Rand.PushSeed();
                Rand.Seed = (pawn.thingIDNumber ^ 63216713);
                if (Rand.Value < joyGiverDef.pctPawnsEverDo)
                {
                    Rand.PopSeed();
                    goto IL_6B;
                }
                Rand.PopSeed();
            IL_A5:
                i++;
                continue;
            IL_6B:
                float num = joyGiverDef.Worker.GetChance(pawn);
                joyGiverChances[joyGiverDef] = num;
                goto IL_A5;
            }
            return allDefsListForReading;
        }
    }
}
