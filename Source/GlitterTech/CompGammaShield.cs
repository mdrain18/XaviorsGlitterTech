using RimWorld;
using Verse;

namespace GlitterTech
{
    public class CompProperties_GammaShield : CompProperties
    {
        public CompProperties_GammaShield()
        {
            this.compClass = typeof(CompGammaShield);
        }
    }

    public class CompGammaShield : CompShield
    {
        // No override needed; logic is handled via Harmony patch
    }
}
