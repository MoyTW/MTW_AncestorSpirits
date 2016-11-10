using Verse;
using Verse.AI.Group;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class PawnVisitInfo : IExposable
    {
        // TODO: Add noteworthy thoughts/reactions here.
        private string pawnString;
        private double approvalDelta;
        private float estDurationDays;
        private bool wasForciblyReturned = false;

        public string PawnString { get { return this.pawnString; } }
        public double ApprovalDelta { get { return this.approvalDelta; } }
        public bool WasForciblyReturned { get { return this.wasForciblyReturned; } }
        public float MaxGain { get { return ApprovalTracker.MaxGainPerDayPerAncestor * this.estDurationDays; } }
        public float MaxLoss { get { return ApprovalTracker.MaxLossPerDayPerAncestor * this.estDurationDays; } }
        // TODO: Bigods this is silly!
        public string MoodSummary
        {
            get
            {
                if (this.approvalDelta > .75f * this.MaxGain)
                {
                    return "impossibly pleased";
                }
                else if (this.approvalDelta > .5f * this.MaxGain)
                {
                    return "extremely pleased";
                }
                else if (this.approvalDelta > .25f * this.MaxGain)
                {
                    return "quite pleased";
                }
                else if (this.approvalDelta > .15f * this.MaxGain)
                {
                    return "moderately pleased";
                }
                else if (this.approvalDelta > .05f * this.MaxGain)
                {
                    return "somewhat pleased";
                }
                else if (this.approvalDelta > .05f * this.MaxLoss)
                {
                    return "mostly indifferent";
                }
                else if (this.approvalDelta > .15f * this.MaxLoss)
                {
                    return "somewhat displeased";
                }
                else if (this.approvalDelta > .25f * this.MaxLoss)
                {
                    return "moderately displeased";
                }
                else if (this.approvalDelta > .5f * this.MaxLoss)
                {
                    return "quite displeased";
                }
                else if (this.approvalDelta > .75f * this.MaxLoss)
                {
                    return "extremely displeased";
                }
                else if (this.approvalDelta > this.MaxLoss)
                {
                    return "impossibly displeased";
                }
                {
                    return "unsure of how to feel";
                }
            }
        }

        public PawnVisitInfo() { }

        public PawnVisitInfo(Pawn p, float estDurationDays)
        {
            this.pawnString = p.ToString();
            this.approvalDelta = 0;
            this.estDurationDays = estDurationDays;
        }

        public void AddApproval(double delta)
        {
            this.approvalDelta += delta;
        }

        public virtual void ExposeData()
        {

            Scribe_Values.LookValue<string>(ref this.pawnString, "pawnString");
            Scribe_Values.LookValue<double>(ref this.approvalDelta, "approvalDelta");
            Scribe_Values.LookValue<float>(ref this.estDurationDays, "estDurationDays");
            Scribe_Values.LookValue<bool>(ref this.wasForciblyReturned, "wasForciblyReturned");
        }
    }

    class AncestralVisitSummary
    {
        public List<Pawn> visitors;
        public List<PawnVisitInfo> visitHistories;

        public AncestralVisitSummary(List<Pawn> visitors, Dictionary<int, PawnVisitInfo> visitInfoMap)
        {
            this.visitors = visitors;
            this.visitHistories = visitInfoMap.Values.ToList();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var info in this.visitHistories)
            {
                builder.AppendFormat("[{0} - {1}] ", info.PawnString, info.ApprovalDelta);
            }
            return builder.ToString();
        }
    }

    class MapCondition_AncestralVisit : MapCondition
    {
        private float estDurationDays;
        private List<Pawn> visitors = new List<Pawn>();
        private Dictionary<int, PawnVisitInfo> visitInfoMap = new Dictionary<int, PawnVisitInfo>();

        public override void Init()
        {
            // Set duration to 1 less than "Permenant" because this will end when visitors despawned
            this.estDurationDays = (float)this.duration / (float)GenDate.TicksPerDay;
            this.duration = 999999999;

            var introDef = DefDatabase<ConceptDef>.GetNamed("MTW_AncestorVisit");
            LessonAutoActivator.TeachOpportunity(introDef, OpportunityType.Important);

            var spawnController = Find.Map.GetComponent<MapComponent_AncestorTicker>();
            for (int i = 0; i < AncestorConstants.ANCESTORS_PER_VISIT; i++)
            {
                var spawned = spawnController.TrySpawnRandomVisitor();
                if (spawned != null)
                {
                    this.visitors.Add(spawned);
                    this.visitInfoMap.Add(spawned.thingIDNumber, new PawnVisitInfo(spawned, this.estDurationDays));
                }
            }

            var loiterPoint = spawnController.CurrentSpawner.Position;
            var lordJob = new LordJob_HauntColony(loiterPoint, this.duration);
            var lord = LordMaker.MakeNewLord(spawnController.AncestorFaction, lordJob, this.visitors);
        }

        public void Notify_DespawnedForAnchorDestruction(Pawn ancestor)
        {
            var loss = ApprovalTracker.PawnApprovalForAnchorDestruction();
            this.visitInfoMap[ancestor.thingIDNumber].AddApproval(loss);
        }

        public void Notify_DespawnedForAnchorBlocked(Pawn ancestor)
        {
            var loss = ApprovalTracker.PawnApprovalForAnchorBlocked();
            this.visitInfoMap[ancestor.thingIDNumber].AddApproval(loss);
        }

        private void PawnApprovalTickInterval(Pawn p)
        {
            float moodDelta = ApprovalTracker.PawnApprovalForInterval(p.needs.mood.CurInstantLevelPercentage);
            this.visitInfoMap[p.thingIDNumber].AddApproval(moodDelta);
        }

        private void SubmitApprovalChanges()
        {
            var summary = new AncestralVisitSummary(this.visitors, this.visitInfoMap);
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_VisitEnded(summary);
        }

        public override void MapConditionTick()
        {
            if (!AncestorUtils.IsIntervalTick()) { return; }

            // Remove despawned visitors
            this.visitors = visitors.Where(p => p.Spawned).ToList();

            if (this.visitors.Count == 0)
            {
                this.duration = 0;
                this.SubmitApprovalChanges();
            }
            else
            {
                foreach (Pawn p in this.visitors)
                {
                    this.PawnApprovalTickInterval(p);
                }
            }
        }

        public override string TooltipString
        {
            get
            {
                // TODO: Use "Displeased" or "Pleased" or something other than numbers!
                StringBuilder builder = new StringBuilder(base.TooltipString);
                builder.AppendLine();
                builder.AppendLine("Ancestors:");
                foreach (var entry in this.visitInfoMap.Values)
                {
                    builder.AppendLine(entry.PawnString + " feels " + entry.MoodSummary);
                }
                return builder.ToString();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.estDurationDays, "estDurationDays");
            Scribe_Collections.LookList<Pawn>(ref this.visitors, "visitors", LookMode.MapReference, new object[0]);
            Scribe_Collections.LookDictionary<int, PawnVisitInfo>(ref this.visitInfoMap, "visitInfoMap", LookMode.Value, LookMode.Deep);
        }
    }
}
