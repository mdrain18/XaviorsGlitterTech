using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    public static class Patch_LongRangeScannerTargets
    {
        static Patch_LongRangeScannerTargets()
        {
            var lumpGen = GenStepDefOf.PreciousLump.genStep as GenStep_PreciousLump;
            if (lumpGen != null && lumpGen.mineables != null)
            {
                ThingDef mineableTitanium = DefDatabase<ThingDef>.GetNamedSilentFail("MineableTitanium");
                if (mineableTitanium != null && !lumpGen.mineables.Contains(mineableTitanium))
                {
                    lumpGen.mineables.Add(mineableTitanium);
                    Log.Message("[GlitterTech] Added MineableTitanium to long-range scanner mineral list.");
                }
            }
            else
            {
                Log.Warning("[GlitterTech] Could not find or patch GenStep_PreciousLump.mineables.");
            }
        }
    }
}
