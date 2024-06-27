using Verse;

namespace GlitterTech
{
    // Settings class for storing mod-specific settings
    public class GlitterTechSettings : ModSettings
    {
        public float statMultiplier = 1.00f; // 100% default
        public bool enableThreeWayBattle = true; // Default enabled
        public float wealthThreshold = 50000f; // Default threshold
        public float pointsMultiplier = 0.75f; // Default points multiplier

        // Method to handle saving/loading of settings
        public override void ExposeData()
        {
            Scribe_Values.Look(ref statMultiplier, "statMultiplier", 1.00f);
            Scribe_Values.Look(ref enableThreeWayBattle, "enableThreeWayBattle", true);
            Scribe_Values.Look(ref wealthThreshold, "wealthThreshold", 50000f);
            Scribe_Values.Look(ref pointsMultiplier, "pointsMultiplier", 0.75f);
            base.ExposeData();
        }
    }
}
