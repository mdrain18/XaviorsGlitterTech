using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [HarmonyPatch(typeof(DeepDrillUtility))]
    public static class DeepDrillRadiusPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(DeepDrillUtility), "GetNextResource",
                new Type[]
                {
                    typeof(IntVec3), typeof(Map),
                    typeof(ThingDef).MakeByRefType(),
                    typeof(int).MakeByRefType(),
                    typeof(IntVec3).MakeByRefType()
                });
        }

        static bool Prefix(IntVec3 p, Map map, ref ThingDef resDef, ref int countPresent, ref IntVec3 cell, ref bool __result)
        {
            // Check if this is our AdvancedDeepDrill
            Building building = map.thingGrid.ThingAt<Building>(p);
            if (building == null || building.def.defName != "AdvancedDeepDrill")
            {
                return true; // let vanilla method run
            }

            // Use custom radius for the advanced drill
            float advancedRadius = 7.0f;
            int cellsToCheck = GenRadial.NumCellsInRadius(advancedRadius);

            for (int i = 0; i < cellsToCheck; i++)
            {
                IntVec3 c = p + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    ThingDef rockDef = map.deepResourceGrid.ThingDefAt(c);
                    if (rockDef != null)
                    {
                        resDef = rockDef;
                        countPresent = map.deepResourceGrid.CountAt(c);
                        cell = c;
                        __result = true;
                        return false; // skip original
                    }
                }
            }

            resDef = null;
            countPresent = 0;
            cell = IntVec3.Invalid;
            __result = false;
            return false; // skip original
        }
    }
}
