using RimWorld;
using Verse;

using System;

namespace MTW_AncestorSpirits
{
    class MemoWorker_GiveThoughtToOneColonist : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            var memoDef = (AncestorMemoDef)this.def;

            Pawn target;
            Find.MapPawns.FreeColonistsSpawned.TryRandomElement(out target);

            if (target != null)
            {
                var thoughtDef = DefDatabase<ThoughtDef>.GetNamed(memoDef.thoughtDef);
                var thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                target.needs.mood.thoughts.memories.TryGainMemoryThought(thought);

                var letterText = String.Format(this.def.letterText, target.LabelShort);
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterType);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
