using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    internal static class ResourceYieldPatch
    {
        static ResourceYieldPatch()
        {
            new Harmony("rimworld.xavior.glittertech").PatchAll();
        }

        [HarmonyPatch(typeof(BuildingProperties), "get_EffectiveMineableYield")]
        public static class BuildingProperties_EffectiveMineableYield
        {
            static void Postfix(ref int __result, BuildingProperties __instance)
            {
                ThingDef thingDef = (from ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading
                                     where def.building != null && def.building.mineableThing == __instance.mineableThing
                                     select def).FirstOrDefault();

                if (thingDef.defName == "Titanium_GT")
                {
                    __result = (int)Math.Round(__result * GlitterTechMod.settings.resourceMultiplier, MidpointRounding.AwayFromZero);
                }
            }
        }
    }
}
