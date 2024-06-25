using HarmonyLib;
using Verse;

namespace GlitterTech.Patches
{
    // Harmony patch to modify resource stats after they are loaded
    [HarmonyPatch(typeof(ThingDef), "PostLoad")]
    static class GT_ResourceStatAdjustmentsPatch
    {
        static void Postfix(ThingDef __instance)
        {
            // Apply changes only to specific ThingDefs
            if (__instance.defName == "BetaPoly" || __instance.defName == "AlphyPoly" || __instance.defName == "Titanium")
            {
                // Adjust each stat base by the current multiplier setting
                foreach (var statBase in __instance.statBases)
                {
                    statBase.value *= GlitterTechMod.settings.statMultiplier;
                }
            }
        }
    }
}
