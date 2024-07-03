using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    internal static class StatBasesPatch
    {
        static StatBasesPatch()
        {
            new Harmony("rimworld.xavior.glittertech").PatchAll();
        }

        [HarmonyPatch(typeof(ThingDef), "PostLoad")]
        public static class ThingDef_PostLoad
        {
            static void Postfix(ThingDef __instance)
            {
                if (__instance.defName == "Titanium_GT")
                {
                    ModifyStatBases(__instance.statBases, GlitterTechMod.settings.statMultiplier);
                }
            }

            private static void ModifyStatBases(List<StatModifier> statBases, float multiplier)
            {
                if (statBases == null) return;

                foreach (var stat in statBases)
                {
                    stat.value *= multiplier;
                }
            }
        }
    }
}
