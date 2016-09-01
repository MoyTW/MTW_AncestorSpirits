using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTW_AncestorSpirits
{
    class AncestorApproval : IExposable
    {
        private enum ShrineStatus
        {
            none = 0,
            one,
            many
        }


        private double approval = 0.0;
        private ShrineStatus previousShrineStatus = ShrineStatus.one;

        private void UpdateApprovalNoShrines()
        {
            if (this.previousShrineStatus != ShrineStatus.none)
            {
                this.previousShrineStatus = ShrineStatus.none;
                // TODO: Not hardcoded strings!
                Find.LetterStack.ReceiveLetter("No Shrines!", "Your Ancestors are displeased that you have no " +
                    "shrines! You will lose approval with them until you build a shrine.", LetterType.BadNonUrgent);
            }
            this.approval += AncestorConstants.APP_MOD_NO_SHRINES_INTERVAL;
        }

        private void UpdateApprovalManyShrines()
        {
            if (this.previousShrineStatus != ShrineStatus.many)
            {
                this.previousShrineStatus = ShrineStatus.many;
                // TODO: Not hardcoded strings!
                Find.LetterStack.ReceiveLetter("Too many Shrines!", "Your Ancestors are displeased that you have " +
                    "too many shrines! You should have one shrine, and one shrine only! You will lose approval with " +
                    " them until you demolish the extras.", LetterType.BadNonUrgent);
            }
            this.approval += AncestorConstants.APP_MOD_MANY_SHRINES_INTERVAL;
        }

        private void UpdateForShrines(MapComponent_AncestorTicker ancestorDriver)
        {
            if (ancestorDriver.CurrentSpawner == null)
            {
                this.UpdateApprovalNoShrines();
            }
            else if (ancestorDriver.NumSpawners > 1)
            {
                this.UpdateApprovalManyShrines();
            }
            else
            {
                this.previousShrineStatus = ShrineStatus.one;
            }
        }

        private void UpdateForPawns(MapComponent_AncestorTicker ancestorDriver)
        {
            var visitingPawns = ancestorDriver.AncestorsVisiting;

            // Pawn approval summary
            foreach (Pawn p in visitingPawns)
            {
                double moodPercent = p.needs.mood.CurInstantLevelPercentage;
                this.approval += (moodPercent / 100) * AncestorConstants.APPROVAL_INTERVAL_MULTIPLIER;
            }
        }

        public void UpdateApproval(MapComponent_AncestorTicker ancestorDriver)
        {
            this.UpdateForPawns(ancestorDriver);
            this.UpdateForShrines(ancestorDriver);

            Log.Message("Approval: " + this.approval.ToString());
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<ShrineStatus>(ref this.previousShrineStatus, "previousShrineStatus", ShrineStatus.one);
            Scribe_Values.LookValue<double>(ref approval, "approval", 0.0);
        }
    }
}
