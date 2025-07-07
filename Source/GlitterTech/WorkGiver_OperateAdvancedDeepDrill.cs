using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace GlitterTech
{
    public class WorkGiver_OperateAdvancedDeepDrill : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("AdvancedDeepDrill"));

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
                return false;

            if (t.IsForbidden(pawn) || !pawn.CanReserve(t))
                return false;

            var comp = t.TryGetComp<AdvancedDeepDrillComp>();
            return comp != null && comp.CanDrillNow();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(GlitterTechJobDefOf.OperateAdvancedDeepDrill, t, 1500, true);
        }
    }
}
