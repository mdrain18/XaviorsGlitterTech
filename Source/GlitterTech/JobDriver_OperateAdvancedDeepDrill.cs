using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GlitterTech
{
    public class JobDriver_OperateAdvancedDeepDrill : JobDriver
    {
        protected Building Building => (Building)this.job.targetA.Thing;
        protected AdvancedDeepDrillComp DrillComp;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            DrillComp = Building.TryGetComp<AdvancedDeepDrillComp>();

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => DrillComp == null || !DrillComp.CanDrillNow());

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            var drill = new Toil();
            drill.tickAction = () =>
            {
                if (DrillComp != null)
                {
                    DrillComp.DrillTick(this.pawn);
                }
            };
            drill.defaultCompleteMode = ToilCompleteMode.Never;
            drill.WithProgressBar(TargetIndex.A, () =>
            {
                return DrillComp?.GetDrillProgressPercent() ?? 0f;
            }, false, -0.5f);

            yield return drill;
        }
    }
}
