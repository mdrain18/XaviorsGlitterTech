using Verse;
using RimWorld;

namespace GlitterTech
{
    [DefOf]
    public static class GlitterTechJobDefOf
    {
        static GlitterTechJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GlitterTechJobDefOf));
        }

        public static JobDef OperateAdvancedDeepDrill;
    }
}
