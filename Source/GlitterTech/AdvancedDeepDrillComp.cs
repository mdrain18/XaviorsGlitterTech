using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace GlitterTech
{
    public class AdvancedDeepDrillComp : ThingComp
    {
        private CompDeepDrill baseComp;

        private const float SpeedMultiplier = 2.0f;
        private const float YieldMultiplier = 1.5f;
        private const float WorkPerPortionBase = 10000f;
        private const float AdvancedDrillRadius = 7.0f;

        private FieldInfo portionProgressField;
        private FieldInfo portionYieldPctField;
        private MethodInfo tryProducePortionMethod;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            baseComp = parent.GetComp<CompDeepDrill>();

            if (baseComp == null)
            {
                Log.Error("AdvancedDeepDrillComp requires a CompDeepDrill to be present on the same Thing.");
                return;
            }

            Type baseType = typeof(CompDeepDrill);
            portionProgressField = baseType.GetField("portionProgress", BindingFlags.NonPublic | BindingFlags.Instance);
            portionYieldPctField = baseType.GetField("portionYieldPct", BindingFlags.NonPublic | BindingFlags.Instance);
            tryProducePortionMethod = baseType.GetMethod("TryProducePortion", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostSpawnSetup(false);
            }
        }

        public bool CanDrillNow()
        {
            var powerComp = this.parent.TryGetComp<CompPowerTrader>();
            return (powerComp == null || powerComp.PowerOn) && ValuableResourcesPresent();
        }

        private bool ValuableResourcesPresent()
        {
            ThingDef resource;
            int count;
            IntVec3 cell;
            return GetNextResource(parent.Position, parent.Map, out resource, out count, out cell);
        }

        private bool GetNextResource(IntVec3 center, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            int cellsToCheck = GenRadial.NumCellsInRadius(AdvancedDrillRadius);
            for (int i = 0; i < cellsToCheck; i++)
            {
                IntVec3 c = center + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    ThingDef rockDef = map.deepResourceGrid.ThingDefAt(c);
                    if (rockDef != null)
                    {
                        resDef = rockDef;
                        countPresent = map.deepResourceGrid.CountAt(c);
                        cell = c;
                        return true;
                    }
                }
            }
            resDef = null;
            countPresent = 0;
            cell = IntVec3.Invalid;
            return false;
        }

        public void DrillTick(Pawn operatorPawn)
        {
            if (baseComp == null) return;

            float workSpeed = operatorPawn.GetStatValue(StatDefOf.MiningSpeed) * SpeedMultiplier;
            float currentProgress = (float)portionProgressField.GetValue(baseComp);

            currentProgress += workSpeed;

            float adjustedWorkNeeded = WorkPerPortionBase / YieldMultiplier;

            if (currentProgress >= adjustedWorkNeeded)
            {
                tryProducePortionMethod.Invoke(baseComp, new object[] { 1f, operatorPawn });
                currentProgress = 0;
            }

            portionProgressField.SetValue(baseComp, currentProgress);
        }

        public float GetDrillProgressPercent()
        {
            if (baseComp == null) return 0f;
            float currentProgress = (float)portionProgressField.GetValue(baseComp);
            float adjustedWorkNeeded = WorkPerPortionBase / YieldMultiplier;
            return Math.Min(currentProgress / adjustedWorkNeeded, 1f);
        }
    }
}
