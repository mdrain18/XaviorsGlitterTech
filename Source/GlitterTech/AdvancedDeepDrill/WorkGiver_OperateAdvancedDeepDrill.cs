using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace GlitterTech
{
    public class WorkGiver_AdvancedDeepDrill : WorkGiver_DeepDrill
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("AdvancedDeepDrill"));

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("OperateAdvancedDeepDrill"), t, 1500, true);
        }
    }
}
