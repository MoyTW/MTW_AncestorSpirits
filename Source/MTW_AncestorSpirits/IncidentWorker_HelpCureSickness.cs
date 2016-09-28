using Verse;
using RimWorld;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_HelpCureSickness : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            var pawns = Find.MapPawns.FreeColonists;
            foreach (var pawn in pawns)
            {
                foreach (var hediff in pawn.health.hediffSet.GetHediffs<HediffWithComps>().ToList())
                {
                    var immunityRecord = pawn.health.immunity.GetImmunityRecord(hediff.def);
                    if (immunityRecord != null)
                    {
                        float immunityGain = .025f + (Rand.Value * .095f);
                        immunityRecord.immunity += immunityGain;
                    }

                    var tendable = hediff.TryGetComp<HediffComp_Tendable>();
                    if (tendable != null)
                    {
                        // Quality of 1.0f is "Well", so this will always tend well.
                        tendable.CompTended(10.0f, 1);
                    }
                }
            }

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType);

            return true;
        }
    }
}
