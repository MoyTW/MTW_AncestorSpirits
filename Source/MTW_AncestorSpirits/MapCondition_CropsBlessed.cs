using Verse;
using RimWorld;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MapCondition_CropsBlessed : MapCondition
    {
        private const int HealInterval = 220;
        private const float DesiredGrowthMultiplier = 2.5f;

        private List<Thing> blessedPlants;

        public override void Init()
        {
            var plants = Find.Map.listerThings.ThingsInGroup(ThingRequestGroup.CultivatedPlant);
            this.blessedPlants = new List<Thing>(plants);
        }

        public override void MapConditionTick()
        {
            for (int i = this.blessedPlants.Count - 1; i >= 0; i--)
            {
                Plant p = (Plant)this.blessedPlants[i];

                if (p.Destroyed)
                {
                    this.blessedPlants.RemoveAt(i);
                }
                else
                {
                    // Accelerate Growth
                    if (p.LifeStage == PlantLifeStage.Growing)
                    {
                        float growthPerTick = (1f / (GenDate.TicksPerDay * p.def.plant.growDays)) * p.GrowthRate;
                        float deltaGrowth = (DesiredGrowthMultiplier - 1f) * growthPerTick;
                        p.Growth += deltaGrowth;
                    }
                    // Heal Minor Damage
                    if (Find.TickManager.TicksGame % HealInterval == 0 && p.HitPoints < p.MaxHitPoints)
                    {
                        p.HitPoints += 1;
                    }
                }
            }
            if (this.blessedPlants.Count == 0)
            {
                this.duration = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.LookList<Thing>(ref this.blessedPlants, "blessedPlants", LookMode.MapReference);
        }

        public override string TooltipString
        {
            get
            {
                return base.TooltipString + "\n\nNumber of currently blessed crops: " + this.blessedPlants.Count;
            }
        }

    }
}
