﻿using Verse;
using Verse.AI.Group;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MapCondition_AncestralVisit : MapCondition
    {
        private List<Pawn> visitors;
        private Dictionary<Pawn, double> approvalMap;

        public override void Init()
        {
            this.visitors = new List<Pawn>();

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
            var lordJob = new LordJob_HauntColony(loiterPoint);
            var lord = LordMaker.MakeNewLord(spawnController.AncestorFaction, lordJob, this.visitors);

            // Set duration to 1 less than "Permenant" because this will end when visitors despawned
            this.duration = 999999999;
        }

        private void PawnApprovalTick(Pawn p)
        {
            // TODO: Implement this!
        }

        public override void MapConditionTick()
        {
            // Remove despawned pawns and early exit if all pawns removed
            this.visitors = visitors.Where(p => p.Spawned).ToList();
            if (this.visitors.Count == 0)
            {
                this.duration = 0;
                return;
            }

            // Track the approval of each pawn here
            foreach (Pawn p in this.visitors)
            {
                this.PawnApprovalTick(p);
            }
        }


    }
}
