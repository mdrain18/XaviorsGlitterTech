using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [HarmonyPatch(typeof(ThingDef), "PostLoad")]
    public static class ResourceYieldPatch
    {
        private static readonly Dictionary<string, int> originalMineableYield = new Dictionary<string, int>();

        static void Postfix(ThingDef __instance)
        {
            if (__instance.defName == "MineableTitanium")
            {
                SaveOriginalValues(__instance);
                ModifyYield(__instance.building, GlitterTechMod.settings.resourceMultiplier);
            }
        }

        public static void ModifyYield(BuildingProperties buildingProps, float multiplier)
        {
            if (buildingProps?.mineableThing != null)
            {
                if (originalMineableYield.TryGetValue(buildingProps.mineableThing.defName, out var originalValue))
                {
                    int newYield = (int)Math.Round(originalValue * multiplier, MidpointRounding.AwayFromZero);
                    //Log.Message($"Modifying yield for {buildingProps.mineableThing.defName}: original = {originalValue}, multiplier = {multiplier}, new = {newYield}");
                    buildingProps.mineableYield = newYield;
                }
                else
                {
                    Log.Warning($"Original mineable yield not found for: {buildingProps.mineableThing.defName}");
                }
            }
        }

        public static void SaveOriginalValues(ThingDef thingDef)
        {
            if (thingDef.building?.mineableThing != null)
            {
                if (!originalMineableYield.ContainsKey(thingDef.building.mineableThing.defName))
                {
                    //Log.Message($"Saving original mineable yield for: {thingDef.building.mineableThing.defName}, yield = {thingDef.building.mineableYield}");
                    originalMineableYield[thingDef.building.mineableThing.defName] = thingDef.building.mineableYield;
                }
            }
        }
    }
}
