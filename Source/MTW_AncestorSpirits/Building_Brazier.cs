using RimWorld;
using Verse;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    [StaticConstructorOnStartup]
    class Building_Brazier : Building
    {
        private CompFlickable flickableComp;
        private static Graphic graphicOn;
        private static Graphic graphicOff;

        public override Graphic Graphic
        {
            get
            {
                if (this.flickableComp.SwitchIsOn)
                {
                    return Building_Brazier.graphicOn;
                }
                return Building_Brazier.graphicOff;
            }
        }

        public bool IsActive { get { return this.flickableComp.SwitchIsOn; } }

        static Building_Brazier()
        {
            graphicOn = GraphicDatabase.Get<Graphic_Single>("PlaceholderBrazierFrontFire");
            graphicOff = GraphicDatabase.Get<Graphic_Single>("PlaceholderBrazierFront");
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.flickableComp = base.GetComp<CompFlickable>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.flickableComp == null)
                {
                    this.flickableComp = base.GetComp<CompFlickable>();
                }
            }
        }

        public override string GetInspectString()
        {
            if (this.flickableComp.SwitchIsOn)
            {
                return "Active: Yes";
            }
            else
            {
                return "Active: No";
            }
        }
    }
}
