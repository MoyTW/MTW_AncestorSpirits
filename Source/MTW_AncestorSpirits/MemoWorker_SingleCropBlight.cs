using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MemoWorker_SingleCropBlight : IncidentWorker
    {
        private const float blightChance = .5f;

        private bool TryBlight(Plant plant)
        {
            if (Rand.Value < blightChance && (plant.LifeStage == PlantLifeStage.Growing || plant.LifeStage == PlantLifeStage.Mature))
            {
                plant.CropBlighted();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TryExecute(IncidentParms parms)
        {
            List<Thing> plantList = Find.Map.listerThings.ThingsInGroup(ThingRequestGroup.CultivatedPlant);

            var blightTarget = plantList.RandomElement();
            if (blightTarget == null) { return false; }

            var blightList = plantList.Where(i => i.def == blightTarget.def).ToList();

            bool didBlightPlant = false;
            for (int i = blightList.Count - 1; i >= 0; i--)
            {
                // Note that this can blight long-growing plants, which normal blights can't!
                if (this.TryBlight((Plant)blightList[i]))
                {
                    didBlightPlant = true;
                }
            }

            if (didBlightPlant)
            {
                var letterText = String.Format(this.def.letterText, blightTarget.LabelShort);
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterType);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
