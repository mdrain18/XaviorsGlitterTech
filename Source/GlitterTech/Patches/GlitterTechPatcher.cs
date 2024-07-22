using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    internal static class GlitterTechPatcher
    {
        static GlitterTechPatcher()
        {
            var harmony = new Harmony("rimworld.xavior.glittertech");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    public static class TraitSet_HasTrait_Patch
    {
        // Define the exact method signature to avoid ambiguity
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(TraitSet), "HasTrait", new Type[] { typeof(TraitDef) });
        }

        static void Postfix(TraitDef tDef, ref bool __result, TraitSet __instance)
        {
            if (tDef == TraitDef.Named("AnnoyingVoice"))
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn.health.hediffSet.HasHediff(HediffDef.Named("GTEar")))
                {
                    __result = false; // Pretend the pawn doesn't have the trait if they have the advanced bionic ear
                }
            }
        }
    }
}
