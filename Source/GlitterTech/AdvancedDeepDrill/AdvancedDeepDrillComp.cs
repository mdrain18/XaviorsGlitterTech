﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace GlitterTech
{
	public class CompProperties_AdvancedDeepDrill : CompProperties
	{
		public float SpeedMultiplier = 2.0f; // 2x speed
		public float YieldMultiplier = 1.5f; // 1.5x yield
		public float WorkPerPortionBase = 10000f; // Base work needed per portion
		public float AdvancedDrillRadius = 7.8f; // Extended drilling radius

		public CompProperties_AdvancedDeepDrill()
		{
			compClass = typeof(AdvancedDeepDrillComp);
		}
	}

	public class AdvancedDeepDrillComp : ThingComp
	{
		public CompProperties_AdvancedDeepDrill Props
		{
			get
			{
				return (CompProperties_AdvancedDeepDrill)props;
			}
		}

		private int numCellsToScan = int.MaxValue;

		public int NumCellsToScan => numCellsToScan != int.MaxValue ? numCellsToScan : numCellsToScan = GenRadial.NumCellsInRadius(Props.AdvancedDrillRadius);


		private CompPowerTrader powerComp;

		private float portionProgress;

		private float portionYieldPct;

		private int lastUsedTick = int.MinValue;


		public float ProgressToNextPortionPercent => portionProgress / 10000f;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			powerComp = parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref portionProgress, "portionProgress", 0f);
			Scribe_Values.Look(ref portionYieldPct, "portionYieldPct", 0f);
			Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", 0);
		}

		public override void PostDeSpawn(Map map)
		{
			portionProgress = 0f;
			portionYieldPct = 0f;
			lastUsedTick = int.MinValue;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Produce portion (100% yield)";
				command_Action.action = delegate
				{
					TryProducePortion(1f);
				};
				yield return command_Action;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (parent.Spawned)
			{
				GetNextResource(out var resDef, out var _, out var _);
				if (resDef == null)
				{
					return "DeepDrillNoResources".Translate();
				}
				return $"{"ResourceBelow".Translate()}: {resDef.LabelCap}\n{"ProgressToNextPortion".Translate()}:{ProgressToNextPortionPercent.ToStringPercent("F0")}";
			}
			return null;
		}

		public bool CanDrillNow()
		{
			GetNextResource(out var resDef, out var _, out var _);
			return (powerComp == null || powerComp.PowerOn) && resDef != null;
		}

		public void DrillWorkDone(Pawn driller)
		{
			float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed) * Props.SpeedMultiplier;
			portionProgress += statValue; // YieldPct should use both multiplier to get the correct yield.
			portionYieldPct += statValue * Props.YieldMultiplier * driller.GetStatValue(StatDefOf.MiningYield) / 10000f;
			lastUsedTick = Find.TickManager.TicksGame;
			if (portionProgress > Props.WorkPerPortionBase)
			{
				TryProducePortion(portionYieldPct, driller);
				portionProgress = 0f;
				portionYieldPct = 0f;
			}
		}

		public bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			return GetNextResource(NumCellsToScan, parent.Position, parent.Map, out resDef, out countPresent, out cell);
		}

		public static bool GetNextResource(int numCellsToScan, IntVec3 p, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			for (int i = 0; i < numCellsToScan; i++)
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
			resDef = DeepDrillUtility.GetBaseResource(map, p);
			countPresent = int.MaxValue;
			cell = p;
			return false;
		}

		private void TryProducePortion(float yieldPct, Pawn driller = null)
		{
			bool valuable = GetNextResource(out var resDef, out var countPresent, out var cell);
			if (resDef == null)
			{
				return;
			}
			int portionCount = Math.Min(resDef.deepCountPerPortion, countPresent);
			var thing = ThingMaker.MakeThing(resDef);
			thing.stackCount = Math.Max(GenMath.RoundRandom(portionCount * yieldPct), 1);
			GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near, null, (IntVec3 p) => p != parent.Position && p != parent.InteractionCell);
			if (valuable)
			{
				parent.Map.deepResourceGrid.SetAt(cell, resDef, countPresent - portionCount);
			}
			if (driller != null)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, driller.Named(HistoryEventArgsNames.Doer)));
			}
			if (valuable && !GetNextResource(out resDef, out _, out _))
			{
				if (resDef == null)
				{
					Messages.Message("DeepDrillExhaustedNoFallback".Translate(), parent, MessageTypeDefOf.TaskCompletion);
					return;
				}
				Messages.Message("DeepDrillExhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(parent.Map, parent.Position).label)), parent, MessageTypeDefOf.TaskCompletion);
				parent.SetForbidden(true);
			}
		}
	}
}
