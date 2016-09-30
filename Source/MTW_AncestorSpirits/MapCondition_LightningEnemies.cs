﻿using Verse;
using RimWorld;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    class MapCondition_LightningEnemies : MapCondition
    {
        private const int minStrikeInterval = GenDate.TicksPerHour / 30;
        private const int maxStrikeInterval = GenDate.TicksPerHour / 15;
        private static readonly IntRange strikeIntervalRange = new IntRange(minStrikeInterval, maxStrikeInterval);

        private const int minRadius = 3;
        private const int maxRadius = 7;
        private static readonly IntRange radiusRange = new IntRange(minRadius, maxRadius);

        private int nextStrikeTicks = 0;

        public override void MapConditionTick()
        {
            if (Find.TickManager.TicksGame > this.nextStrikeTicks)
            {
                var target = Find.MapPawns.AllPawnsSpawned
                    .Where(p => p.HostileTo(Faction.OfPlayer))
                    .Where(p => !p.Position.Roofed())
                    .RandomElement();

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
                    this.nextStrikeTicks = Find.TickManager.TicksGame + strikeIntervalRange.RandomInRange;
                }
            }
        }

        private bool IsGoodLocationForStrike(IntVec3 pos)
        {
            return pos.InBounds() && !pos.Roofed() && pos.Standable();
        }

    }
}