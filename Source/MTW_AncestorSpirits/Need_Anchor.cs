using RimWorld;
using Verse;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class Need_Anchor : Need
    {
        // TODO: Derive these from "Number of hours before you must return to your anchor"
        private const float baseGainPerTickRate = 150f;
        private const float baseFallPerTick = 1E-05f;
        private const float threshLow = 0.2f;
        private const float minStart = .9f;
        private const float maxStart = .99f;

        private int lastGainTick;

        static Need_Anchor() { }

        public bool IsLow { get { return this.CurLevel < threshLow; } }

        public override int GUIChangeArrow
        {
            get
            {
                return this.GainingNeed ? 1 : -1;
            }
        }

        private bool GainingNeed
        {
            get
            {
                return Find.TickManager.TicksGame < this.lastGainTick + 10;
            }
        }

        public Need_Anchor(Pawn pawn) : base(pawn)
        {
            this.lastGainTick = -999;
            this.threshPercents = new List<float>();
            this.threshPercents.Add(threshLow);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = Rand.Range(minStart, maxStart);
        }

        public void GainNeed(float amount)
        {
            amount /= 120f;
            amount *= 0.01f;
            amount = Mathf.Min(amount, 1f - this.CurLevel);
            this.curLevelInt += amount;
            this.lastGainTick = Find.TickManager.TicksGame;
        }

        public override void NeedInterval()
        {
            if (!this.GainingNeed)
            {
                this.curLevelInt -= (baseFallPerTick * baseGainPerTickRate);
            }
            if (this.curLevelInt >= 0.0)
            {
                return;
            }
            this.curLevelInt = 0.0f;
        }

        public override string GetTipString()
        {
            return base.GetTipString();
        }

    }
}
