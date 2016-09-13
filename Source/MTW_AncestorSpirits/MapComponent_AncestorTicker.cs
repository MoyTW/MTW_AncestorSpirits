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

        // Magic
        public const int MAGIC_START = 6;

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

        /* HACK ALERT - See the Notify_ForceWeather function for details. */
        private int ticksToKeepWeather;
        private WeatherDef forcedWeatherDef;

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

        public int NumSpawners { get { return this.spawners.Count; } }

        // I have no idea of the perf implications of these functions!
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

        public void Notify_PetitionMade(PetitionDef petition, Pawn petitioner)
        {
            if (this.approval.CurrentMagic >= petition.magicCost)
            {
                Messages.Message("The Ancestors have heard your petition!", MessageSound.Benefit);
                var incidentParams = new IncidentParms();
                incidentParams.forced = true;
                petition.Worker.TryExecute(incidentParams);
                this.approval.SubtractMagic(petition.magicCost);
            }
            else
            {
                Messages.Message("You haven't enough magic! The Ancestors have rejected your petition.", MessageSound.Negative);
            }
        }

        public void Notify_ForceWeather(WeatherDef weatherDef, int durationTicks)
        {
            this.ticksToKeepWeather = durationTicks;
            this.forcedWeatherDef = weatherDef;
        }
        #endregion

        #region Overrides

        /* HACK ALERT
         * Because it's not possible to manually set the duration of a weather event through any defs other than the
         * WeatherDef (the variable in the WeatherDecider which controls when it transitions, curWeatherDuration, is
         * private and cannot be assigned to through functions) we trick the system into thinking that the weather
         * effect has never passed 4000 ticks by constantly resetting it to 4000.
         * 
         * The reason we set it to 4000 is because that is how many ticks it takes for the weather to complete
         * transitioning from the previous weather to the current one (rainfall takes time to ramp up).
         * 
         * Here is how the weather loops works:
         * + WeatherDecider starts a new weather cycle. It decides how long this cycle should last using the def, and
         *   stores it in its curWeatherDuration variable.
         * + WeatherDecider tells the WeatherManager to transition to the new weather, and sets curWeatherAge = 0
         * + Every tick, curWeatherAge increments
         * + When curWeatherAge > curWeatherDuration, WeatherDecider decides on a new weather
         * 
         * Therefore, we set curWeatherAge (which for some reason is public) to 4000 until our targeted ticks have
         * passed, thereby freezing the weather.
         * 
         * It could be broken off into its own Component I suppose, but I'll deal with that if it becomes an issue.
         */
        private void ForceWeather()
        {
            if (this.forcedWeatherDef == Find.WeatherManager.curWeather && this.ticksToKeepWeather > 0)
            {
                if (Find.WeatherManager.curWeatherAge > 4000)
                {
                    Find.WeatherManager.curWeatherAge = 4000;
                }
                this.ticksToKeepWeather--;
            }
        }

        public override void MapComponentTick()
        {
            this.ForceWeather();

            // No Rare version of MapComponentTick, so this will do.
            if (!(Find.TickManager.TicksGame % AncestorConstants.TICKS_PER_INTERVAL == 0)) { return; }
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
            // TODO: Fix the loading errors!
            // Scribe_Collections.LookList<Pawn>(ref this.unspawnedAncestors, "unspawnedAncestors", LookMode.Deep, new object[0]);
            Scribe_Collections.LookHashSet<Building>(ref this.spawners, "spawners", LookMode.MapReference);
            Scribe_References.LookReference<Building>(ref this._currentSpawner, "currentSpawner");
        }

        #endregion
    }
}
