﻿using RimWorld;
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
        public const int TICKS_PER_SEASON = 45000;
        public const int TICK_INTERVAL = 250;
        public const double SEASONS_PER_INTERVAL = (double)TICK_INTERVAL / (double)TICKS_PER_SEASON;

        public const int MIN_ANCESTORS = 10;

        public const double MAX_APPROVAL_PER_ANCESTOR_PER_SEASON = 2;
        public const double APPROVAL_INTERVAL_MULTIPLIER = SEASONS_PER_INTERVAL * MAX_APPROVAL_PER_ANCESTOR_PER_SEASON;

        public const double APP_MOD_NO_SHRINES_PER_SEASON = -8;
        public const double APP_MOD_NO_SHRINES_INTERVAL = SEASONS_PER_INTERVAL * APP_MOD_NO_SHRINES_PER_SEASON;

        public const double APP_MOD_MANY_SHRINES_PER_SEASON = -4;
        public const double APP_MOD_MANY_SHRINES_INTERVAL = SEASONS_PER_INTERVAL * APP_MOD_MANY_SHRINES_PER_SEASON;
    }

    class MapComponent_AncestorTicker : MapComponent
    {
        private Random rand = new Random();

        private Faction _faction = null;
        private Building _currentSpawner = null;

        private bool initialized = false;
        private List<Pawn> unspawnedAncestors = new List<Pawn>();
        private HashSet<Building> spawners = new HashSet<Building>();
        private AncestorApproval approval = null;

        private int numAncestorsToVisit = 3;

        #region Properties

        public Faction AncestorFaction
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

        public Building CurrentSpawner
        {
            get
            {
                // Technically, _currentSpawner can be orphaned.
                if (!this.spawners.Any()) { return null; }

                if (!this.spawners.Contains<Building>(this._currentSpawner))
                {
                    this._currentSpawner = this.spawners.First();
                }

                return this._currentSpawner;
            }
        }

        public int NumSpawners
        {
            get
            {
                return this.spawners.Count;
            }
        }

        // I have no idea of the perf implications of these functions!
        public IEnumerable<Pawn> AncestorsVisiting
        {
            get
            {
                return Find.MapPawns.PawnsInFaction(this.AncestorFaction);
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
            this.approval = new AncestorApproval();
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
            if (this.CurrentSpawner == null) { return false; }

            IntVec3 pos;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(this.CurrentSpawner, out pos);
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
                visitor.GetLord().Notify_PawnLost(visitor, PawnLostCondition.Vanished);
                this.unspawnedAncestors.Add(visitor);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Notifications
        public void Notify_SpawnerCreated(Building spawner)
        {
            this.spawners.Add(spawner);
        }

        public void Notify_SpawnerDestroyed(Building spawner)
        {
            this.spawners.Remove(spawner);
            if (this.CurrentSpawner != null)
            {
                foreach (var ancestor in this.AncestorsVisiting)
                {
                    ancestor.GetLord().Notify_PawnLost(ancestor, PawnLostCondition.Vanished);
                }
                var loiterPoint = this.CurrentSpawner.Position;
                var lordJob = new LordJob_DefendPoint(loiterPoint);
                LordMaker.MakeNewLord(this.AncestorFaction, lordJob, this.AncestorsVisiting);
            }
        }
        #endregion

        #region Overrides

        public override void MapComponentTick()
        {
            // No Rare version of MapComponentTick, so this will do.
            if (!(Find.TickManager.TicksGame % AncestorConstants.TICK_INTERVAL == 0)) { return; }
            if (!this.initialized) { this.Initialize(); }

            if (this.CurrentSpawner == null)
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
                var loiterPoint = this.CurrentSpawner.Position;
                var lordJob = new LordJob_DefendPoint(loiterPoint);
                LordMaker.MakeNewLord(this.AncestorFaction, lordJob, this.AncestorsVisiting);
            }

            this.approval.UpdateApproval(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<bool>(ref initialized, "initialized", false);
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Deep.LookDeep<AncestorApproval>(ref this.approval, "approval", new object[0]);
            Scribe_Collections.LookList<Pawn>(ref this.unspawnedAncestors, "unspawnedAncestors", LookMode.Deep, new object[0]);
            Scribe_Collections.LookHashSet<Building>(ref this.spawners, "spawners", LookMode.MapReference);
            Scribe_References.LookReference<Building>(ref this._currentSpawner, "currentSpawner");
        }

        #endregion
    }
}
