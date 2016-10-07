using Verse;
using Verse.AI.Group;
using RimWorld;

using System;
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

        public string PawnString { get { return this.pawnString; } }
        public double ApprovalDelta { get { return this.approvalDelta; } }
        public string MoodSummary
        {
            get
            {
                if (this.approvalDelta > 0)
                {
                    return "Positive! :)";
                }
                else
                {
                    return "Negative. :(";
                }
            }
        }

        public PawnVisitInfo() { }

        public PawnVisitInfo(Pawn p)
        {
            this.pawnString = p.ToString();
            this.approvalDelta = 0;
        }

        public void AddApproval(double delta)
        {
            this.approvalDelta += delta;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.LookValue<string>(ref this.pawnString, "pawnString");
            Scribe_Values.LookValue<double>(ref this.approvalDelta, "approvalDelta");
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
    }

    class MapCondition_AncestralVisit : MapCondition
    {
        private List<Pawn> visitors = new List<Pawn>();
        private Dictionary<int, PawnVisitInfo> visitInfoMap = new Dictionary<int, PawnVisitInfo>();

        public override void Init()
        {
            var spawnController = Find.Map.GetComponent<MapComponent_AncestorTicker>();
            for (int i = 0; i < AncestorConstants.ANCESTORS_PER_VISIT; i++)
            {
                var spawned = spawnController.TrySpawnRandomVisitor();
                if (spawned != null)
                {
                    this.visitors.Add(spawned);
                    this.visitInfoMap.Add(spawned.thingIDNumber, new PawnVisitInfo(spawned));
                }
            }

            var loiterPoint = spawnController.CurrentSpawner.Position;
            var lordJob = new LordJob_HauntColony(loiterPoint, this.duration);
            var lord = LordMaker.MakeNewLord(spawnController.AncestorFaction, lordJob, this.visitors);

            // Set duration to 1 less than "Permenant" because this will end when visitors despawned
            this.duration = 999999999;
        }

        private void PawnApprovalTick(Pawn p)
        {
            double curMoodPercent = p.needs.mood.CurInstantLevelPercentage - AncestorConstants.APP_NEG_CUTOFF;
            if (curMoodPercent > 0)
            {
                this.visitInfoMap[p.thingIDNumber].AddApproval(curMoodPercent * AncestorConstants.APP_MULT_GAIN_PER_SEASON);
            }
            else
            {
                this.visitInfoMap[p.thingIDNumber].AddApproval(curMoodPercent * AncestorConstants.APP_MULT_LOSS_PER_SEASON);
            }
        }

        private void SubmitApprovalChanges()
        {
            var summary = new AncestralVisitSummary(this.visitors, this.visitInfoMap);
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_VisitEnded(summary);
        }

        public override void MapConditionTick()
        {
            // Remove despawned pawns and early exit if all pawns removed
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
                    this.PawnApprovalTick(p);
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
            Scribe_Collections.LookList<Pawn>(ref this.visitors, "visitors", LookMode.MapReference, new object[0]);
            Scribe_Collections.LookDictionary<int, PawnVisitInfo>(ref this.visitInfoMap, "visitInfoMap", LookMode.Value, LookMode.Deep);
        }
    }
}
