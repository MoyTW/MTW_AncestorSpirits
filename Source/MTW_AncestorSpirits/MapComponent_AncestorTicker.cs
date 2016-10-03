using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public static class AncestorConstants
    {
        public const int TICKS_PER_INTERVAL = GenDate.TicksPerHour / 10;
        public const double INTERVALS_PER_SEASON = (double)GenDate.TicksPerSeason / (double)TICKS_PER_INTERVAL;

        public const int MIN_ANCESTORS = 10;

        public const int ANCESTORS_PER_VISIT = 3;

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
        public const int EVENT_TIMER_TICKS_BEFORE_FIRST = (int)(EVENT_TIMER_DAYS_BEFORE_FIRST * GenDate.TicksPerDay);

        public const double EVENT_TIMER_DAYS_BETWEEN = 7.0;
        public const int EVENT_TIMER_TICKS_BETWEEN = (int)(EVENT_TIMER_DAYS_BETWEEN * GenDate.TicksPerDay);
        public const double EVENT_TIMER_HOURS_PLUS_MINUS = 48.0;
        public const int EVENT_TIMER_TICKS_PLUS_MINUS = (int)(EVENT_TIMER_HOURS_PLUS_MINUS * GenDate.TicksPerHour);

        public const double EVENT_TRIGGER_GAIN_SEASON_DELTA = 5.0;
        public const double EVENT_TRIGGER_GAIN_INTERVAL_DELTA = EVENT_TRIGGER_GAIN_SEASON_DELTA / INTERVALS_PER_SEASON;
        public const double EVENT_TRIGGER_LOSS_SEASON_DELTA = -6.0; // TODO: Consistency between pos/negatives here
        public const double EVENT_TRIGGER_LOSS_INTERVAL_DELTA = EVENT_TRIGGER_LOSS_SEASON_DELTA / INTERVALS_PER_SEASON;

        public const double EVENT_TIMER_MIN_OMEN_HOURS = 1.0;
        public const int EVENT_TIMER_MIN_OMEN_TICKS = (int)(EVENT_TIMER_MIN_OMEN_HOURS * GenDate.TicksPerHour);
        public const double EVENT_TIMER_MAX_OMEN_HOURS = 6.0;
        public const int EVENT_TIMER_MAX_OMEN_TICKS = (int)(EVENT_TIMER_MAX_OMEN_HOURS * GenDate.TicksPerHour);

        /* Magic & Petitions
         * The petition base success chance is listed here. The min multiplier is how much less you can spent to
         * achieve the base success chance, and how much you have to overspend to achieve 100%. Success is linear from
         * the base chance up to cost*(1 + min_multi).
         */
        public const int MAGIC_START = 6;
        public const float PETITION_BASE_SUCCESS = .60f;
        public const float PETITION_SPEND_MIN_MULT = .5f;

        #endregion
    }

    class MapComponent_AncestorTicker : MapComponent
    {
        private Random rand = new Random();

        private Faction _faction = null;

        private bool initialized = false;
        private List<Pawn> unspawnedAncestors = new List<Pawn>();
        private List<Pawn> despawnBuffer = new List<Pawn>();
        private List<Building> spawners = new List<Building>();
        private ApprovalTracker approval = null;
        private AncestorEdictTimer timer = null;

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
                return this.spawners.LastOrDefault<Building>();
            }
        }

        public int NumSpawners { get { return this.spawners.Count; } }

        public IEnumerable<Pawn> AncestorsVisiting
        {
            get
            {
                return Find.MapPawns.PawnsInFaction(this.AncestorFaction);
            }
        }

        public int CurrentMagic { get { return this.approval.CurrentMagic; } }

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
            this.approval = new ApprovalTracker();
            this.timer = new AncestorEdictTimer();
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

        public Pawn TrySpawnRandomVisitor()
        {
            if (this.CurrentSpawner == null) { return null; }

            IntVec3 pos;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(this.CurrentSpawner, out pos);
            if (pos == null) { return null; }

            var visitor = this.PopOrGenUnspawnedPawn();
            GenSpawn.Spawn(visitor, pos);

            var anchorNeed = visitor.needs.TryGetNeed<Need_Anchor>();
            if (anchorNeed != null)
            {
                anchorNeed.SetToMax();
            }

            return visitor;
        }

        private bool DespawnVisitor(Pawn visitor)
        {
            if (visitor != null)
            {
                visitor.GetLord().Notify_PawnLost(visitor, PawnLostCondition.Vanished);
                visitor.DeSpawn();
                if (this.despawnBuffer.Contains(visitor))
                {
                    this.despawnBuffer.Remove(visitor);
                }
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
                var lordJob = new LordJob_HauntColony(loiterPoint);
                LordMaker.MakeNewLord(this.AncestorFaction, lordJob, this.AncestorsVisiting);
            }
        }

        public void Notify_PetitionMade(PetitionDef petition, Pawn petitioner)
        {
            var magicUsed = ((Building_Shrine)this.CurrentSpawner).MagicForNextRitual;
            if (magicUsed == 0)
            {
                Messages.Message("You cannot petition your Ancestors without using magic! Are your braziers lit?",
                    MessageSound.Negative);
                return;
            }
            if (magicUsed > this.approval.CurrentMagic)
            {
                Messages.Message("Your magic was insufficient to even begin the petition! No magic has been used.",
                    MessageSound.Negative);
                return;
            }

            var successTarget = Rand.Value;
            var successChance = petition.SuccesChance(magicUsed);
            if (successChance == 0.0f)
            {
                // TODO: PUNISH for this?
                Messages.Message("Your offerings were insulting! The Ancestors have rejected your petition.",
                    MessageSound.Negative);
            }
            else if (successTarget >= successChance)
            {
                Messages.Message("The Ancestors have not heard your petition - or if they have, they are silent.",
                    MessageSound.Negative);
            }
            else
            {
                Messages.Message("The Ancestors have heard your petition!", MessageSound.Benefit);
                var incidentParams = new IncidentParms();
                incidentParams.forced = true;
                petition.Worker.TryExecute(incidentParams);
            }

            this.approval.SubtractMagic(magicUsed);
            Messages.Message("You have spent " + magicUsed + ". Your magic is now " + this.approval.CurrentMagic,
                MessageSound.Standard);
        }

        // Kinda messy. Should work out how you want state of visitors to be handled.
        public void Notify_ShouldDespawn(Pawn ancestor)
        {
            this.DespawnVisitor(ancestor);
        }

        #endregion

        #region Overrides

        public override void MapComponentTick()
        {
            // No Rare version of MapComponentTick, so this will do.
            if (!(Find.TickManager.TicksGame % AncestorConstants.TICKS_PER_INTERVAL == 0)) { return; }
            if (!this.initialized) { this.Initialize(); }

            foreach (Pawn visitor in this.despawnBuffer)
            {
                this.DespawnVisitor(visitor);
            }

            if (this.CurrentSpawner == null)
            {
                foreach (Pawn visitor in this.AncestorsVisiting)
                {
                    this.DespawnVisitor(visitor);
                }
            }
            else if (this.AncestorsVisiting.Count() < numAncestorsToVisit && this.AncestorsVisiting.Count() == 0)
            {
                while (this.AncestorsVisiting.Count() < numAncestorsToVisit)
                {
                    this.TrySpawnRandomVisitor();
                }
                var loiterPoint = this.CurrentSpawner.Position;
                var lordJob = new LordJob_HauntColony(loiterPoint);
                LordMaker.MakeNewLord(this.AncestorFaction, lordJob, this.AncestorsVisiting);
            }

            this.approval.ApprovalTrackerTickInterval(this);
            this.timer.EdictTimerTickInterval(this.approval);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<bool>(ref initialized, "initialized", false);
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Deep.LookDeep<ApprovalTracker>(ref this.approval, "approval", new object[0]);
            Scribe_Deep.LookDeep<AncestorEdictTimer>(ref this.timer, "timer", new object[0]);
            Scribe_Collections.LookList<Pawn>(ref this.unspawnedAncestors, "unspawnedAncestors", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList<Building>(ref this.spawners, "spawners", LookMode.MapReference);
        }

        #endregion
    }
}
