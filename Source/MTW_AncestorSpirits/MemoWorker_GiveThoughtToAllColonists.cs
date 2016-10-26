using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MemoWorker_GiveThoughtToAllColonists : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            var memoDef = (AncestorMemoDef)this.def;

            var pawns = Find.MapPawns.FreeColonistsSpawned;
            foreach (Pawn p in pawns)
            {
                var thoughtDef = DefDatabase<ThoughtDef>.GetNamed(memoDef.thoughtDef);
                var thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                p.needs.mood.thoughts.memories.TryGainMemoryThought(thought);
            }

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType);

            return true;
        }
    }
}
