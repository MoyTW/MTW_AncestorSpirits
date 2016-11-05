using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class CompSpiritGraphics : ThingComp
    {
        private bool hasSet = false;

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is Pawn)
            {
                if (!this.hasSet)
                {
                    AncestorUtils.SetAncestorGraphics((Pawn)this.parent);
                    this.hasSet = true;
                }
            }
            else
            {
                Log.ErrorOnce("Cannot add SpiritGraphics to non-Pawn Thing " + this.parent.Label, 65447891);
            }
        }
    }
}
