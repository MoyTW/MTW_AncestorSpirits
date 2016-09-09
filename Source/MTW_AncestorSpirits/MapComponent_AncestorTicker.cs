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
        public const int TICKS_PER_SEASON = 450000;
        public const int TICKS_PER_HOUR = 1250;
        public const int TICKS_PER_DAY = TICKS_PER_HOUR * 24;
        public const int TICK_INTERVAL = TICKS_PER_HOUR / 5;
        public const double INTERVALS_PER_SEASON = (double)TICKS_PER_SEASON / (double)TICK_INTERVAL;

        public const int MIN_ANCESTORS = 10;

        #region Approval

        public const int APPROVAL_HISTORY_HOURS = 24 * 7;

        /* Approval System
         * Cutoff is the percentage approval at which they switch from negative to positive.
         *
         * Negative/Positive gains are capped at MAX_APP/LOSS_PER_ANCESTOR_PER_SEASON. The multiplier is computed from
         * the cutoff, the number of intervals per season, and the max gain/loss.
         *  */
        public const double APP_NEG_CUTOFF = 0.4;

        public const double MAX_APP_GAIN_PER_ANCESTOR_PER_SEASON = 2.0;
        public const double APP_MULT_GAIN_PER_SEASON = (MAX_APP_GAIN_PER_ANCESTOR_PER_SEASON / (1.0 - APP_NEG_CUTOFF)) / INTERVALS_PER_SEASON;

        public const double MAX_APP_LOSS_PER_ANCESTOR_PER_SEASON = 1.0;
        public const double APP_MULT_LOSS_PER_SEASON = (MAX_APP_LOSS_PER_ANCESTOR_PER_SEASON / APP_NEG_CUTOFF) / INTERVALS_PER_SEASON;

        public const double APP_MOD_NO_SHRINES_PER_SEASON = -8.0;
        public const double APP_MOD_NO_SHRINES_INTERVAL = APP_MOD_NO_SHRINES_PER_SEASON / INTERVALS_PER_SEASON;

        public const double APP_MOD_MANY_SHRINES_PER_SEASON = -4.0;
        public const double APP_MOD_MANY_SHRINES_INTERVAL = APP_MOD_MANY_SHRINES_PER_SEASON / INTERVALS_PER_SEASON;

        /* Events Triggers
         * There are two conditions for an event trigger - time, or deltas
         * 
         * Once an initial time period has passed, the game becomes eligible for events. An event is scheduled with a 
         * TTL ticker, and when the timer ticks down it will fire. Then, the process resets.
         * 
         * Alternatively, if there's a delta within an hour which exceeds the maximum allowed delta, an event will fire
         * "soon". The upper/lower bound for this time is set by MIN_OMEN_HOURS/MAX_OMEN_HOURS. This will also reset 
         * the ticker to a new value, and no delta-triggered events may fire until the next scheduled event.
         * */

        public const double EVENT_TIMER_DAYS_BEFORE_FIRST = 7.0;

        public const double EVENT_TIMER_DAYS_BETWEEN = 7.0;
        public const double EVENT_TIMER_TICKS_BETWEEN = EVENT_TIMER_DAYS_BETWEEN * TICKS_PER_DAY;
        public const double EVENT_TIMER_HOURS_PLUS_MINUS = 48.0;
        public const double EVENT_TIMER_TICKS_PLUS_MINUS = EVENT_TIMER_HOURS_PLUS_MINUS * TICKS_PER_HOUR;

        public const double EVENT_TRIGGER_GAIN_SEASON_DELTA = 5.0;
        public const double EVENT_TRIGGER_GAIN_INTERVAL_DELTA = EVENT_TRIGGER_GAIN_SEASON_DELTA / INTERVALS_PER_SEASON;
        public const double EVENT_TRIGGER_LOSS_SEASON_DELTA = -6.0; // TODO: Consistency between pos/negatives here
        public const double EVENT_TRIGGER_LOSS_INTERVAL_DELTA = EVENT_TRIGGER_LOSS_SEASON_DELTA / INTERVALS_PER_SEASON;

        public const double EVENT_TIMER_MIN_OMEN_HOURS = 1.0;
        public const int EVENT_TIMER_MIN_OMEN_TICKS = (int)(EVENT_TIMER_MIN_OMEN_HOURS * TICKS_PER_HOUR);
        public const double EVENT_TIMER_MAX_OMEN_HOURS = 6.0;
        public const int EVENT_TIMER_MAX_OMEN_TICKS = (int)(EVENT_TIMER_MAX_OMEN_HOURS * TICKS_PER_HOUR);

        #endregion
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
        private EventTimer timer = null;

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
            this.timer = new EventTimer();
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
            this.timer.UpdateTimer(this.approval);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<bool>(ref initialized, "initialized", false);
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Deep.LookDeep<AncestorApproval>(ref this.approval, "approval", new object[0]);
            Scribe_Deep.LookDeep<EventTimer>(ref this.timer, "timer", new object[0]);
            Scribe_Collections.LookList<Pawn>(ref this.unspawnedAncestors, "unspawnedAncestors", LookMode.Deep, new object[0]);
            Scribe_Collections.LookHashSet<Building>(ref this.spawners, "spawners", LookMode.MapReference);
            Scribe_References.LookReference<Building>(ref this._currentSpawner, "currentSpawner");
        }

        #endregion
    }
}
