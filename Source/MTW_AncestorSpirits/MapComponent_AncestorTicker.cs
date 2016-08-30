using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

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

        private Faction _faction = null;

        private bool initialized = false;
        private List<Pawn> unspawnedAncestors = new List<Pawn>();
        private int numAncestorsToVisit = 3;

        private double approval = 0.0;

        #region Properties

        private Faction AncestorFaction
        {
            get
            {
                if (this._faction == null)
                {
                    this._faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("MTW_Ancestor"));
                }
                return this._faction;
            }
        }

        // I have no idea of the perf implications of these functions!
        private IEnumerable<Pawn> AncestorsVisiting
        {
            get
            {
                return Find.MapPawns.PawnsInFaction(this.AncestorFaction);
            }
        }

        private IEnumerable<Thing> Spawners
        {
            get
            {
                // TODO: Figure out why using allBuildingsColonistOfDef will hard-crash the game.
                // TODO: Why does LINQ cause egregious loading errors!?
                return Find.ListerThings.ThingsOfDef(ThingDef.Named("MTW_AncestorShrine"));
            }
        }

        #endregion

        #region Pawn Manipulation

        private Pawn GenAncestor()
        {
            PawnKindDef pawnKindDef = PawnKindDef.Named("AncestorSpirit");
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, this.AncestorFaction);
            return PawnGenerator.GeneratePawn(request);
        }

        private void Initialize()
        {
            while (this.unspawnedAncestors.Count() < AncestorConstants.MIN_ANCESTORS)
            {
                this.unspawnedAncestors.Add(this.GenAncestor());
            }
            this.initialized = true;
        }

        private Pawn PopOrGenUnspawnedPawn()
        {
            if (!this.unspawnedAncestors.Any())
            {
                return this.GenAncestor();
            }
            else
            {
                var pawn = this.unspawnedAncestors[Rand.Range(0, this.unspawnedAncestors.Count)];
                this.unspawnedAncestors.Remove(pawn);
                return pawn;
            }
        }

        private Pawn GetVisitingPawn()
        {
            var onMap = this.AncestorsVisiting;
            if (!onMap.Any())
            {
                return null;
            }
            else
            {
                return onMap.RandomElement();
            }
        }

        private bool TrySpawnRandomVisitor()
        {
            var spawners = this.Spawners;
            if (!spawners.Any()) { return false; }

            Thing spawner = spawners.RandomElement<Thing>();
            if (spawner == null) { return false; }

            IntVec3 pos;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(spawner, out pos);
            if (pos == null) { return false; }

            GenSpawn.Spawn(this.PopOrGenUnspawnedPawn(), pos);
            return true;
        }

        private bool DespawnRandomVisitor()
        {
            var visitor = this.GetVisitingPawn();
            if (visitor != null)
            {
                visitor.DeSpawn();
                this.unspawnedAncestors.Add(visitor);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        private void UpdateApproval()
        {
            var visitingPawns = this.AncestorsVisiting;

            foreach (Pawn p in visitingPawns)
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
            if (!this.initialized) { this.Initialize(); }

            if (!this.Spawners.Any())
            {
                while (this.AncestorsVisiting.Count() > 0)
                {
                    this.DespawnRandomVisitor();
                }
            }
            else if (this.AncestorsVisiting.Count() < numAncestorsToVisit)
            {
                while (this.AncestorsVisiting.Count() < numAncestorsToVisit)
                {
                    this.TrySpawnRandomVisitor();
                }
                var loiterPoint = this.Spawners.First().Position;
                var lordJob = new LordJob_DefendPoint(loiterPoint);
                LordMaker.MakeNewLord(this.AncestorFaction, lordJob, this.AncestorsVisiting);
            }

            this.UpdateApproval();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref initialized, "initialized", false);
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Values.LookValue<double>(ref approval, "approval", 0.0);
            Scribe_Collections.LookList<Pawn>(ref this.unspawnedAncestors, "unspawnedAncestors", LookMode.Deep, new object[0]);
        }

        #endregion
    }
}
