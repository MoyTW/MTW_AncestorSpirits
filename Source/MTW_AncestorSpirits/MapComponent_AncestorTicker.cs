using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTW_AncestorSpirits
{
    class MapComponent_AncestorTicker : MapComponent
    {
        public override void MapComponentTick()
        {
            Log.Message("Ancestor info is loaded!");
        }
    }
}
