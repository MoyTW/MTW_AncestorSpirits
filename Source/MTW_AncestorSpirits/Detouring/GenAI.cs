using RimWorld;
using Verse;
using Source = Verse.AI.GenAI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MTW_AncestorSpirits.Detouring
{
    static class GenAI
    {
        [Detour(typeof(Source), bindingFlags = BindingFlags.Public | BindingFlags.Static)]
        public static bool CanBeArrested(this Pawn pawn)
        {
            if (pawn.def.defName == "Spirit")
            {
                return false;
            }
            // Below is the old code
            return pawn.RaceProps.Humanlike && !pawn.InAggroMentalState && !pawn.HostileTo(Faction.OfPlayer) && (!pawn.IsPrisonerOfColony || !pawn.Position.IsInPrisonCell());
        }
    }
}
