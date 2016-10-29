using RimWorld;
using Verse;

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
        public int MagicContributed { get { return this.IsActive ? 1 : 0; } }

        static Building_Brazier()
        {
            graphicOn = GraphicDatabase.Get<Graphic_Single>("PlaceholderBrazierFrontFire");
            graphicOff = GraphicDatabase.Get<Graphic_Single>("PlaceholderBrazierFront");
        }

        public override void SpawnSetup()
        {
            var introDef = DefDatabase<ConceptDef>.GetNamed("MTW_AncestorBrazierBuilt");
            LessonAutoActivator.TeachOpportunity(introDef, OpportunityType.Important);

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

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            if (signal == "FlickedOff" || signal == "FlickedOn")
            {
                Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things, true, false);
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
