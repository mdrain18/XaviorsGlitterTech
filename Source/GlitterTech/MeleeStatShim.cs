using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    /// <summary>
    /// 1) Extra AP multiplier for melee: __result *= weapon.GetStatValue(MeleeWeapon_ArmorPenetrationMultiplier)
    /// 2) Optional absolute melee cooldown override: if weapon.GetStatValue(MeleeWeapon_Cooldown) > 0 -> use it
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MeleeStatShim
    {
        static readonly StatDef APStat =
            DefDatabase<StatDef>.GetNamedSilentFail("MeleeWeapon_ArmorPenetrationMultiplier");

        static readonly StatDef CooldownOverrideStat =
            DefDatabase<StatDef>.GetNamedSilentFail("MeleeWeapon_Cooldown");

        // ---- Patch 1/2: Armor Penetration (tool overload covers all real uses) ----
        [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration),
            new[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) })]
        public static class Patch_MeleeAP
        {
            public static void Postfix(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource, ref float __result)
            {
                if (equipment == null || APStat == null) return;
                // Multiply armor pen by weapon stat (default 1 => no change)
                float mult = equipment.GetStatValue(APStat);
                if (mult != 1f)
                    __result *= mult;
            }
        }

        // ---- Patch 2/2: Cooldown (patch Tool.AdjustedCooldown since everything funnels through it) ----
        [HarmonyPatch(typeof(Tool), nameof(Tool.AdjustedCooldown), new[] { typeof(Thing) })]
        public static class Patch_MeleeCooldown
        {
            public static void Postfix(Thing ownerEquipment, ref float __result)
            {
                if (ownerEquipment == null || CooldownOverrideStat == null) return;

                // If a weapon defines an absolute melee cooldown (> 0), use it.
                float overrideVal = ownerEquipment.GetStatValue(CooldownOverrideStat);
                if (overrideVal > 0.001f)
                    __result = overrideVal;
            }
        }
    }
}
