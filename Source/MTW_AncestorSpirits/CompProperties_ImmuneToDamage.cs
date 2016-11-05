using Verse;

namespace MTW_AncestorSpirits
{
    class CompProperties_ImmuneToDamage : CompProperties
    {
        public CompProperties_ImmuneToDamage()
        {
            this.compClass = typeof(CompImmuneToDamage);
        }
    }
}
