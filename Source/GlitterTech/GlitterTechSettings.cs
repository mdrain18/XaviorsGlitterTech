using Verse;

namespace GlitterTech
{
    // Settings class for storing mod-specific settings
    public class GlitterTechSettings : ModSettings
    {
        public float statMultiplier = 1.00f; // 100% default

        // Method to handle saving/loading of settings
        public override void ExposeData()
        {
            Scribe_Values.Look(ref statMultiplier, "statMultiplier", 1.00f);
            base.ExposeData();
        }
    }
}
