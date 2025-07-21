using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [HarmonyPatch(typeof(CompShield), nameof(CompShield.PostPreApplyDamage))]
    public static class GammaShieldDamagePatch
    {
        public static void Prefix(ref DamageInfo dinfo, CompShield __instance)
        {
            if (__instance?.parent?.def?.defName == "Apparel_GammaShieldBelt" &&
                dinfo.Def.defName == "BeamBypassShields" &&
                ModsConfig.IsActive("ludeon.rimworld.odyssey"))
            {
                // Temporarily override the ignoreShields flag
                AccessTools.FieldRefAccess<DamageInfo, bool>(ref dinfo, "ignoreShields") = false;
            }
        }
    }
}
