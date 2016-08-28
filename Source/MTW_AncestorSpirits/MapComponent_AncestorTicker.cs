using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Time Reference:
 * In-Game Time         Ticks
 * 1 Hour 		        1,250
 * 1 Day (24 Hours) 	30,000
 * 1 Season (15 Days) 	450,000
 * 1 Year (4 Seasons) 	1,800,000
 */

namespace MTW_AncestorSpirits
{
    public static class AncestorConstants
    {
        public const int MIN_ANCESTORS = 10;
        public const double MAX_APPROVAL_PER_ANCESTOR_PER_SEASON = 2;
        public const int TICKS_PER_SEASON = 45000;
        public const int TICK_INTERVAL = 250;
        public const double APPROVAL_INTERVAL_MULTIPLIER = (double)TICK_INTERVAL / (double)TICKS_PER_SEASON * MAX_APPROVAL_PER_ANCESTOR_PER_SEASON;
    }

    class MapComponent_AncestorTicker : MapComponent
    {
        private Random rand = new Random();

        private List<Pawn> ancestors = new List<Pawn>();
        private List<Thing> spawners = new List<Thing>();

        private int numAncestorsToVisit = 3;
        private int numAncestorsVisiting = 0;

        private double approval = 0.0;

        #region Registration

        public void RegisterAncestorSpawner(Thing spawner)
        {
            if (!this.spawners.Contains(spawner))
            {
                this.spawners.Add(spawner);
            }
        }

        public void DeregisterAncestorSpawner(Thing spawner)
        {
            this.spawners.Remove(spawner);
        }

        #endregion

        private Pawn GenAncestor()
        {
            // TODO: Add a Ancestor faction!
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            PawnKindDef pawnKindDef = PawnKindDef.Named("AncestorSpirit");
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, faction);
            Pawn ancestor = PawnGenerator.GeneratePawn(request);
            this.ancestors.Add(ancestor);

            return ancestor;
        }

        private Pawn GetUnspawnedPawn()
        {
            List<Pawn> unspawned = new List<Pawn>();
            foreach (Pawn p in this.ancestors)
            {
                if (!p.Spawned)
                {
                    unspawned.Add(p);
                }
            }

            if (unspawned.Count == 0)
            {
                return this.GenAncestor();
            }
            else
            {
                return unspawned[Rand.Range(0, unspawned.Count)];

            }
        }

        private Pawn GetSpawnedPawn()
        {
            List<Pawn> spawned = new List<Pawn>();
            foreach (Pawn p in this.ancestors)
            {
                if (p.Spawned) { spawned.Add(p); }
            }
            return spawned.RandomElement();
        }

        private bool TrySpawnRandomVisitor()
        {
            Thing spawner;
            spawners.TryRandomElement(out spawner);
            if (spawner == null) { return false; }

            IntVec3 pos;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(spawner, out pos);
            if (pos == null) { return false; }

            GenSpawn.Spawn(this.GetUnspawnedPawn(), pos);
            this.numAncestorsVisiting++;
            return true;
        }

        private bool DespawnRandomVisitor()
        {
            var spawned = this.GetSpawnedPawn();
            if (spawned != null)
            {
                spawned.DeSpawn();
                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerable<Pawn> GetSpawnedPawns()
        {
            List<Pawn> spawned = new List<Pawn>();
            foreach (Pawn p in this.ancestors)
            {
                if (p.Spawned) { spawned.Add(p); }
            }
            return spawned;
        }

        private void UpdateApproval()
        {
            var spawnedPawns = this.GetSpawnedPawns();

            foreach (Pawn p in spawnedPawns)
            {
                double moodPercent = p.needs.mood.CurInstantLevelPercentage;
                this.approval += (moodPercent / 100) * AncestorConstants.APPROVAL_INTERVAL_MULTIPLIER;
            }
            Log.Message("Approval: " + this.approval.ToString());
        }

        #region Overrides

        public override void MapComponentTick()
        {
            // No Rare version of MapComponentTick, so this will do.
            if (!(Find.TickManager.TicksGame % AncestorConstants.TICK_INTERVAL == 0)) { return; }

            if (!this.spawners.Any())
            {
                this.DespawnRandomVisitor();
            }
            else if (numAncestorsVisiting < numAncestorsToVisit)
            {
                this.TrySpawnRandomVisitor();
            }

            this.UpdateApproval();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Values.LookValue<int>(ref numAncestorsVisiting, "numAncestorsVisiting", 0);
            Scribe_Values.LookValue<double>(ref approval, "approval", 0.0);
            Scribe_Collections.LookList<Pawn>(ref ancestors, "ancestors", LookMode.DefReference);
            Scribe_Collections.LookList<Thing>(ref spawners, "spawners", LookMode.DefReference);
        }

        #endregion
    }
}
