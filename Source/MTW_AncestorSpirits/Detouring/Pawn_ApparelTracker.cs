using RimWorld;
using Verse;
using Source = RimWorld.Pawn_ApparelTracker;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MTW_AncestorSpirits.Detouring
{
    internal static class Pawn_ApparelTracker
    {
        [Detour(typeof(Source), bindingFlags = BindingFlags.Public | BindingFlags.Instance)]
        public static bool get_PsychologicallyNude(this Source _this)
        {
            if (AncestorUtils.IsAncestor(_this.pawn)) { return false; } // New Condition

            // Existing Decompiled
            if (_this.pawn.gender == Gender.None)
            {
                return false;
            }
            bool flag;
            bool flag2;
            _this.HasBasicApparel(out flag, out flag2);
            if (_this.pawn.gender == Gender.Male)
            {
                return !flag;
            }
            return _this.pawn.gender == Gender.Female && (!flag || !flag2);
        }

    }
}
