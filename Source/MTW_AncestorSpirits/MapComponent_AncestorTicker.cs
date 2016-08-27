using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTW_AncestorSpirits
{
    class MapComponent_AncestorTicker : MapComponent
    {
        private Random rand = new Random();

        private List<Pawn> ancestors = new List<Pawn>();
        private List<Thing> spawners = new List<Thing>();

        private int minAncestors = 10;
        private int numAncestorsToVisit = 3;
        private int numAncestorsVisiting = 0;

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
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction,
                PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, null, null,
                null, null, null, null);
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

        #region Overrides

        public override void MapComponentTick()
        {
            // No Rare version of MapComponentTick, so this will do.
            if (!(Find.TickManager.TicksGame % 250 == 0)) { return; }

            if (!this.spawners.Any())
            {
                this.DespawnRandomVisitor();
            }
            else if (numAncestorsVisiting < numAncestorsToVisit)
            {
                this.TrySpawnRandomVisitor();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref minAncestors, "minAncestors", 10);
            Scribe_Values.LookValue<int>(ref numAncestorsToVisit, "numAncestorsToVisit", 3);
            Scribe_Values.LookValue<int>(ref numAncestorsVisiting, "numAncestorsVisiting", 0);
            Scribe_Collections.LookList<Pawn>(ref ancestors, "ancestors", LookMode.DefReference);
            Scribe_Collections.LookList<Thing>(ref spawners, "spawners", LookMode.DefReference);
        }

        #endregion
    }
}
