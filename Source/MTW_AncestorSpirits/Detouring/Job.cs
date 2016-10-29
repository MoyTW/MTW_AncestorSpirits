using RimWorld;
using Verse;
using Verse.AI;
using Source = Verse.AI.Job;
using System.Reflection;

namespace MTW_AncestorSpirits.Detouring
{
    internal static class Job
    {
        [Detour(typeof(Source), bindingFlags = BindingFlags.Public | BindingFlags.Instance)]
        public static void ExposeData(this Source _this)
        {
            // Hack!
            if (_this.GetType() == typeof(Job_PetitionAncestors))
            {
                ((Job_PetitionAncestors)_this).ExposeAdditionalData();
            }
            // Existing
            ILoadReferenceable loadReferenceable = (ILoadReferenceable)_this.commTarget;
            Scribe_References.LookReference<ILoadReferenceable>(ref loadReferenceable, "commTarget", false);
            _this.commTarget = (ICommunicable)loadReferenceable;
            Scribe_References.LookReference<Verb>(ref _this.verbToUse, "verbToUse", false);
            Scribe_References.LookReference<Bill>(ref _this.bill, "bill", false);
            Scribe_Defs.LookDef<JobDef>(ref _this.def, "def");
            Scribe_TargetInfo.LookTargetInfo(ref _this.targetA, "targetA");
            Scribe_TargetInfo.LookTargetInfo(ref _this.targetB, "targetB");
            Scribe_TargetInfo.LookTargetInfo(ref _this.targetC, "targetC");
            Scribe_Collections.LookList<TargetInfo>(ref _this.targetQueueA, "targetQueueA", LookMode.Undefined, new object[0]);
            Scribe_Collections.LookList<TargetInfo>(ref _this.targetQueueB, "targetQueueB", LookMode.Undefined, new object[0]);
            Scribe_Collections.LookList<int>(ref _this.numToBringList, "numToBring", LookMode.Undefined, new object[0]);
            Scribe_Collections.LookList<ThingStackPart>(ref _this.placedThings, "placedThings", LookMode.Undefined, new object[0]);
            Scribe_Values.LookValue<int>(ref _this.startTick, "startTick", -1, false);
            Scribe_Values.LookValue<int>(ref _this.expiryInterval, "expiryInterval", -1, false);
            Scribe_Values.LookValue<int>(ref _this.maxNumMeleeAttacks, "maxNumMeleeAttacks", 2147483647, false);
            Scribe_Values.LookValue<bool>(ref _this.exitMapOnArrival, "exitMapOnArrival", false, false);
            Scribe_Values.LookValue<bool>(ref _this.killIncappedTarget, "killIncappedTarget", false, false);
            Scribe_Values.LookValue<int>(ref _this.maxNumToCarry, "maxNumToHaul", -1, false);
            Scribe_Values.LookValue<bool>(ref _this.haulOpportunisticDuplicates, "haulOpportunisticDuplicates", false, false);
            Scribe_Values.LookValue<HaulMode>(ref _this.haulMode, "haulMode", HaulMode.Undefined, false);
            Scribe_Defs.LookDef<ThingDef>(ref _this.plantDefToSow, "plantDefToSow");
            Scribe_Values.LookValue<bool>(ref _this.playerForced, "playerForced", false, false);
            Scribe_Values.LookValue<LocomotionUrgency>(ref _this.locomotionUrgency, "locomotionUrgency", LocomotionUrgency.Jog, false);
            Scribe_Values.LookValue<bool>(ref _this.applyAnesthetic, "applyAnesthetic", false, false);
            Scribe_Values.LookValue<bool>(ref _this.ignoreDesignations, "ignoreDesignations", false, false);
            Scribe_Values.LookValue<bool>(ref _this.checkOverrideOnExpire, "checkOverrideOnExpire", false, false);
            Scribe_Values.LookValue<bool>(ref _this.canBash, "canBash", false, false);
            Scribe_Values.LookValue<bool>(ref _this.haulDroppedApparel, "haulDroppedApparel", false, false);
            Scribe_Values.LookValue<bool>(ref _this.restUntilHealed, "restUntilHealed", false, false);
            Scribe_Values.LookValue<bool>(ref _this.ignoreJoyTimeAssignment, "ignoreJoyTimeAssignment", false, false);
            Scribe_Values.LookValue<bool>(ref _this.overeat, "overeat", false, false);
            Scribe_Values.LookValue<bool>(ref _this.attackDoorIfTargetLost, "attackDoorIfTargetLost", false, false);
            Scribe_Values.LookValue<int>(ref _this.takeExtraIngestibles, "takeExtraIngestibles", 0, false);
            Scribe_Values.LookValue<bool>(ref _this.expireRequiresEnemiesNearby, "expireRequiresEnemiesNearby", false, false);
        }
    }
}
