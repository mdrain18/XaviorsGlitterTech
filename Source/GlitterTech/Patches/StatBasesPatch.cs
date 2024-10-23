using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [HarmonyPatch(typeof(ThingDef), "PostLoad")]
    public static class StatBasesPatch
    {
        private static readonly Dictionary<string, Dictionary<StatDef, float>> originalStatBases = new Dictionary<string, Dictionary<StatDef, float>>();
        private static readonly Dictionary<string, Dictionary<StatDef, float>> originalStatFactors = new Dictionary<string, Dictionary<StatDef, float>>();

        static void Postfix(ThingDef __instance)
        {
            if (__instance.defName == "Titanium_GT" || __instance.defName == "AlphaPoly" || __instance.defName == "BetaPoly")
            {
                SaveOriginalValues(__instance);
                ModifySpecificStats(__instance, GlitterTechMod.settings.statMultiplier);
            }
        }

        public static void SaveOriginalValues(ThingDef thingDef)
        {
            if (!originalStatBases.ContainsKey(thingDef.defName))
            {
                var statBaseValues = new Dictionary<StatDef, float>();
                foreach (var statBase in thingDef.statBases)
                {
                    statBaseValues[statBase.stat] = statBase.value;
                    //Log.Message($"Saving original stat base value for {thingDef.defName} - {statBase.stat.defName}: {statBase.value}");
                }
                originalStatBases[thingDef.defName] = statBaseValues;
            }

            if (!originalStatFactors.ContainsKey(thingDef.defName))
            {
                var statFactorValues = new Dictionary<StatDef, float>();
                foreach (var statFactor in thingDef.stuffProps.statFactors)
                {
                    statFactorValues[statFactor.stat] = statFactor.value;
                    //Log.Message($"Saving original stat factor value for {thingDef.defName} - {statFactor.stat.defName}: {statFactor.value}");
                }
                originalStatFactors[thingDef.defName] = statFactorValues;
            }
        }

        public static void ModifySpecificStats(ThingDef thingDef, float multiplier)
        {
            if (originalStatBases.TryGetValue(thingDef.defName, out var originalBaseValues))
            {
                foreach (var statBase in thingDef.statBases)
                {
                    if (originalBaseValues.TryGetValue(statBase.stat, out var originalValue))
                    {
                        statBase.value = originalValue * multiplier;
                        //Log.Message($"Modifying stat base for {thingDef.defName} - {statBase.stat.defName}: original = {originalValue}, multiplier = {multiplier}, new = {statBase.value}");
                    }
                }
            }
            else
            {
                Log.Warning($"Original stat base values not found for: {thingDef.defName}");
            }

            if (originalStatFactors.TryGetValue(thingDef.defName, out var originalFactorValues))
            {
                foreach (var statFactor in thingDef.stuffProps.statFactors)
                {
                    if (originalFactorValues.TryGetValue(statFactor.stat, out var originalValue))
                    {
                        statFactor.value = originalValue * multiplier;
                        //Log.Message($"Modifying stat factor for {thingDef.defName} - {statFactor.stat.defName}: original = {originalValue}, multiplier = {multiplier}, new = {statFactor.value}");
                    }
                }
            }
            else
            {
                Log.Warning($"Original stat factor values not found for: {thingDef.defName}");
            }
        }
    }
}
