using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MemoWorker_PregnantAnimal : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            // I don't know of a good way to find faction - playerTribe/Colony is weird. Could be nicer.
            var someColonist = Find.MapPawns.FreeColonists.FirstOrDefault();
            if (someColonist == null) { return false; }

            var unpregnantColonyAnimals = Find.MapPawns.SpawnedPawnsInFaction(someColonist.Faction)
                .Where(p => p.RaceProps.Animal && !p.health.hediffSet.HasHediff(HediffDefOf.Pregnant));
            if (unpregnantColonyAnimals.Count() == 0) { return false; }

            var target = unpregnantColonyAnimals.RandomElementByWeight(p => p.MarketValue);

            var hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, target, null);
            target.health.AddHediff(hediff_Pregnant, null, null);

            var letterText = String.Format(this.def.letterText, target.LabelShort, target.KindLabel);
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterType);
            return true;
        }
    }
}
