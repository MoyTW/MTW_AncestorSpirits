using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class Building_Shrine : Building
    {
        private CompAffectedByFacilities facilitiesComp;

        public int MagicForNextRitual
        {
            get
            {
                return this.facilitiesComp.LinkedFacilitiesListForReading
                    .Sum(n => ((Building_Brazier)n).MagicContributed);
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_SpawnerCreated(this);
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.facilitiesComp = base.GetComp<CompAffectedByFacilities>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.facilitiesComp == null)
                {
                    this.facilitiesComp = base.GetComp<CompAffectedByFacilities>();
                }
            }
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_SpawnerDestroyed(this);
        }

        public override string GetInspectString()
        {
            int magic = Find.Map.GetComponent<MapComponent_AncestorTicker>().CurrentMagic;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Magic available: " + magic);
            builder.AppendLine("Magic for next ritual: " + this.MagicForNextRitual);
            builder.Append(base.GetInspectString());

            return builder.ToString();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!myPawn.CanReserve(this, 1))
            {
                FloatMenuOption disallowed = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
                return new List<FloatMenuOption> { disallowed };
            }
            else if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
            {
                FloatMenuOption disallowed = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
                return new List<FloatMenuOption> { disallowed };
            }
            else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                FloatMenuOption disallowed = new FloatMenuOption("CannotUseReason".Translate(new object[]
                {
                    "IncapableOfCapacity".Translate(new object[]
                    {
                        PawnCapacityDefOf.Talking.label
                    })
                }), null, MenuOptionPriority.Medium, null, null, 0f, null);
                return new List<FloatMenuOption> { disallowed };
            }

            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (PetitionDef petition in DefDatabase<PetitionDef>.AllDefs)
            {
                Action action = delegate
                {
                    Job job = new Job_PetitionAncestors(DefDatabase<JobDef>.GetNamed("PetitionAncestors"), this, petition);
                    myPawn.drafter.TakeOrderedJob(job);
                };
                list.Add(new FloatMenuOption(petition.PetitionSummary, action, MenuOptionPriority.Medium, null, null, 0f, null));
            }

            return list;
        }
    }
}
