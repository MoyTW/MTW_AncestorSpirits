using RimWorld;
using Verse;
using Verse.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class JobGiver_InspectColony : JobGiver_WanderColony
    {
        public override float GetPriority(Pawn pawn)
        {
            return Rand.Range(.95f, 1.05f);
        }
    }
}
