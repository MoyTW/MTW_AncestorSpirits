using Verse;
using Verse.AI.Group;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class AncestralVisitSummary
    {
        public List<Pawn> visitors;
        public Dictionary<Pawn, double> approvalMap;

        public AncestralVisitSummary(List<Pawn> visitors, Dictionary<Pawn, double> approvalMap)
        {
            this.visitors = visitors;
            this.approvalMap = approvalMap;
        }
    }

    class MapCondition_AncestralVisit : MapCondition
    {
        private List<Pawn> visitors;
        private Dictionary<Pawn, double> approvalMap;

        public override void Init()
        {
            this.visitors = new List<Pawn>();
            this.approvalMap = new Dictionary<Pawn, double>();

            var spawnController = Find.Map.GetComponent<MapComponent_AncestorTicker>();
            for (int i = 0; i < AncestorConstants.ANCESTORS_PER_VISIT; i++)
            {
                var spawned = spawnController.TrySpawnRandomVisitor();
                if (spawned != null)
                {
                    this.visitors.Add(spawned);
                    this.approvalMap.Add(spawned, 0.0);
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
                this.approvalMap[p] += curMoodPercent * AncestorConstants.APP_MULT_GAIN_PER_SEASON;
            }
            else
            {
                this.approvalMap[p] += curMoodPercent * AncestorConstants.APP_MULT_LOSS_PER_SEASON;
            }
        }

        private void SubmitApprovalChanges()
        {
            var summary = new AncestralVisitSummary(this.visitors, this.approvalMap);
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
                foreach (var entry in this.approvalMap)
                {
                    builder.AppendLine(entry.Key + ": " + entry.Value);
                }
                return builder.ToString();
            }
        }
    }
}
