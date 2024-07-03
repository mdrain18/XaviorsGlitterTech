using UnityEngine;
using Verse;

namespace GlitterTech
{
    public class GlitterTechSettings : ModSettings
    {
        public bool enableThreeWayBattle = true;
        public float wealthThreshold = 50000f;
        public float pointsMultiplier = 0.75f;
        public float resourceMultiplier = 1f;
        public float statMultiplier = 1f;  

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableThreeWayBattle, "enableThreeWayBattle", true);
            Scribe_Values.Look(ref wealthThreshold, "wealthThreshold", 50000f);
            Scribe_Values.Look(ref pointsMultiplier, "pointsMultiplier", 0.75f);
            Scribe_Values.Look(ref resourceMultiplier, "resourceMultiplier", 1f);
            Scribe_Values.Look(ref statMultiplier, "statMultiplier", 1f);  
            base.ExposeData();
        }
    }
}
