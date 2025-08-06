using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [HarmonyPatch(typeof(CompShield), nameof(CompShield.PostPreApplyDamage))]
    public static class GammaShieldDamagePatch
    {
        public static void Prefix(CompShield __instance, DamageInfo dinfo)
        {
            if (__instance?.parent?.def?.defName == "Apparel_GammaShieldBelt" &&
                dinfo.Def.defName == "BeamBypassShields" &&
                ModsConfig.IsActive("ludeon.rimworld.odyssey"))
            {
                // Force override 'ignoreShields' via Traverse
                Traverse.Create(dinfo).Field("ignoreShields").SetValue(false);
            }
        }
    }
}
