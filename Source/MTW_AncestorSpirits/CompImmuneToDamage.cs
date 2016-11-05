using Verse;

namespace MTW_AncestorSpirits
{
    class CompImmuneToDamage : ThingComp
    {
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            absorbed = true;
        }
    }
}
