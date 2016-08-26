using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

/*
public class IncidentWorker_RefugeePodCrash : IncidentWorker
{
    private const float FogClearRadius = 4.5f;

    private const float RelationWithColonistWeight = 20f;

    public override bool TryExecute(IncidentParms parms)
    {
        IntVec3 intVec = DropCellFinder.RandomDropSpot();
        Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
        PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, null, null, null, null, null, null);
        Pawn pawn = PawnGenerator.GeneratePawn(request);
        HealthUtility.GiveInjuriesToForceDowned(pawn);
        string label = "LetterLabelRefugeePodCrash".Translate();
        string text = "RefugeePodCrash".Translate();
        PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
        Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, intVec, null);
        DropPodUtility.MakeDropPodAt(intVec, new DropPodInfo
        {
            SingleContainedThing = pawn,
            openDelay = 180,
            leaveSlag = true
        });
        return true;
    }
}
*/

namespace MTW_AncestorSpirits
{
    public class Building_Shrine : Building
    {
        private List<Pawn> ancestors = new List<Pawn>();

        private Pawn SpawnAncestorAdjacent()
        {
            IntVec3 adj;
            GenAdj.TryFindRandomWalkableAdjacentCell8Way(this, out adj);
            
            // TODO: Add a Ancestor faction!
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction,
                PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, null, null,
                null, null, null, null);
            Pawn ancestor = PawnGenerator.GeneratePawn(request);

            GenSpawn.Spawn(ancestor, adj);
            return ancestor;
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (!this.ancestors.Any())
            {
                Pawn ancestor = this.SpawnAncestorAdjacent();
                this.ancestors.Add(ancestor);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (var ancestor in this.ancestors)
            {
                ancestor.Destroy(DestroyMode.Vanish);
            }

            base.Destroy(mode);
        }
    }
}
