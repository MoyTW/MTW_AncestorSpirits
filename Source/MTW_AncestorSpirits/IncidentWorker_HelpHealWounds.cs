using Verse;
using RimWorld;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class IncidentWorker_HelpHealWounds : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            var pawns = Find.MapPawns.FreeColonists;
            foreach (var pawn in pawns)
            {
                foreach (var hediff in pawn.health.hediffSet.GetHediffs<Hediff_Injury>())
                {
                    if (!hediff.IsOld() &&
                        (hediff.IsNaturallyHealing() || hediff.NotNaturallyHealingBecauseNeedsTending()))
                    {
                        int healValue = 1 + Mathf.RoundToInt(Rand.Value * 4.0f);
                        pawn.health.HealHediff(hediff, healValue);
                    }
                }
            }

            if (this.def.letterLabel != null && this.def.letterText != null)
            {
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType);
            }

            return true;
        }
    }
}
