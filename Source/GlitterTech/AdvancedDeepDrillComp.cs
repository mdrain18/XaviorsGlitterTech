using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace GlitterTech
{
    public class AdvancedDeepDrillComp : ThingComp
    {
        private CompDeepDrill baseComp;

        private const float SpeedMultiplier = 2.0f; // 2x speed
        private const float YieldMultiplier = 1.5f; // 1.5x yield
        private const float WorkPerPortionBase = 10000f; // Base work needed per portion
        private const int AdvancedDrillRadius = 7; // Extended drilling radius

        private FieldInfo portionProgressField;
        private FieldInfo portionYieldPctField;
        private FieldInfo lastUsedTickField;
        private MethodInfo tryProducePortionMethod;

        public bool CanDrillNow()
        {
            var powerComp = this.parent.TryGetComp<CompPowerTrader>();
            return (powerComp == null || powerComp.PowerOn) && (GetBaseResource(this.parent.Map, this.parent.Position) != null || ValuableResourcesPresent());
        }

        public bool ValuableResourcesPresent()
        {
            ThingDef thingDef;
            int countPresent;
            IntVec3 cell;
            return GetNextResource(this.parent.Position, this.parent.Map, out thingDef, out countPresent, out cell);
        }

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
            lastUsedTickField = baseType.GetField("lastUsedTick", BindingFlags.NonPublic | BindingFlags.Instance);
            tryProducePortionMethod = baseType.GetMethod("TryProducePortion", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private float PortionProgress
        {
            get => (float)portionProgressField.GetValue(baseComp);
            set => portionProgressField.SetValue(baseComp, value);
        }

        private float PortionYieldPct
        {
            get => (float)portionYieldPctField.GetValue(baseComp);
            set => portionYieldPctField.SetValue(baseComp, value);
        }

        private int LastUsedTick
        {
            get => (int)lastUsedTickField.GetValue(baseComp);
            set => lastUsedTickField.SetValue(baseComp, value);
        }

        private void TryProducePortion(float yieldPct, Pawn driller)
        {
            tryProducePortionMethod.Invoke(baseComp, new object[] { yieldPct, driller });
        }

        public void CustomDrillWorkDone(Pawn driller)
        {
            if (baseComp == null)
            {
                Log.Error("CompDeepDrill is null in AdvancedDeepDrillComp.");
                return;
            }

            float baseSpeed = driller.GetStatValue(StatDefOf.DeepDrillingSpeed, true);
            PortionProgress += baseSpeed * SpeedMultiplier; 
            PortionYieldPct += (baseSpeed * driller.GetStatValue(StatDefOf.MiningYield, true) / 10000f) * YieldMultiplier; 
            LastUsedTick = Find.TickManager.TicksGame;

            if (PortionProgress >= WorkPerPortionBase)
            {
                TryProducePortion(PortionYieldPct, driller);
                PortionProgress = 0f;
                PortionYieldPct = 0f;
            }
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Produce portion (100% yield)",
                    action = () => CustomDrillWorkDone(null) // For testing purposes, you can pass null or a test pawn
                };
            }
        }

        public override string CompInspectStringExtra()
        {
            if (baseComp == null || !parent.Spawned)
            {
                return null;
            }

            if (GetNextResource(this.parent.Position, this.parent.Map, out ThingDef thingDef, out int countPresent, out IntVec3 cell))
            {
                return $"ResourceBelow: {thingDef.LabelCap}\nProgressToNextPortion: {(PortionProgress / WorkPerPortionBase).ToStringPercent("F0")}";
            }

            return "DeepDrillNoResources".Translate();
        }


        private bool GetNextResource(IntVec3 p, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            for (int i = 0; i < GenRadial.NumCellsInRadius(AdvancedDrillRadius); i++)
            {
                IntVec3 intVec = p + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map))
                {
                    ThingDef thingDef = map.deepResourceGrid.ThingDefAt(intVec);
                    if (thingDef != null)
                    {
                        resDef = thingDef;
                        countPresent = map.deepResourceGrid.CountAt(intVec);
                        cell = intVec;
                        return true;
                    }
                }
            }
            resDef = GetBaseResource(map, p);
            countPresent = int.MaxValue;
            cell = p;
            return false;
        }

        private ThingDef GetBaseResource(Map map, IntVec3 cell)
        {
            if (!map.Biome.hasBedrock)
            {
                return null;
            }
            Rand.PushState();
            Rand.Seed = cell.GetHashCode();
            ThingDef result = (from rock in Find.World.NaturalRockTypesIn(map.Tile)
                               select rock.building.mineableThing).RandomElement();
            Rand.PopState();
            return result;
        }
    }
}
