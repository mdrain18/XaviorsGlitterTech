using RimWorld;
using Verse;

namespace GlitterTech;

public class PlaceWorker_AdvancedDeepDrill : PlaceWorker_DeepDrill
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore, Thing thing)
	{
		AdvancedDeepDrillComp.GetNextResource(GenRadial.NumCellsInRadius(checkingDef.specialDisplayRadius), loc, map, out var resDef, out var _, out var _);
		if (resDef == null)
		{
			return "MustPlaceOnDrillable".Translate();
		}
		return true;
	}
}