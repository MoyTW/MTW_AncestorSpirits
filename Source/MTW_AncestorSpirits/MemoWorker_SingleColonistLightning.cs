using RimWorld;
using Verse;
using UnityEngine;

using System;
using System.Linq;

namespace MTW_AncestorSpirits
{
    class MemoWorker_SingleColonistLightning : IncidentWorker
    {
        private static readonly IntRange radiusRange = new IntRange(1, 2);

        public override bool TryExecute(IncidentParms parms)
        {
            var colonistsOutside = Find.MapPawns.FreeColonistsSpawned
                .Where(p => !p.RaceProps.Animal)
                .Where(p => !p.Position.Roofed() && !p.Downed)
                .ToList();
            if (colonistsOutside.Count == 0) { return false; }

            bool hasFired = false;
            int fireAttempts = 0;

            // Arbitary limit here
            while (!hasFired && fireAttempts < 25)
            {
                Pawn target = colonistsOutside.RandomElement();

                // This shares similarity with LightningEnemies; could pull into own fn!
                float offsetRadius = (float)radiusRange.RandomInRange;
                Vector2 offset = new Vector2(Rand.Gaussian(0f, 1f), Rand.Gaussian(0f, 1f));
                offset.Normalize();
                offset *= Rand.Range(0f, offsetRadius);
                IntVec3 lightningPos = new IntVec3(
                    (int)Math.Round((double)offset.x) + target.Position.x,
                    0,
                    (int)Math.Round((double)offset.y) + target.Position.z);

                if (this.IsGoodLocationForStrike(lightningPos))
                {
                    Find.WeatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(lightningPos));
                    hasFired = true;
                }
                else
                {
                    fireAttempts++;
                }
            }

            if (hasFired)
            {
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsGoodLocationForStrike(IntVec3 pos)
        {
            return pos.InBounds() && !pos.Roofed() && pos.Standable();
        }
    }
}
