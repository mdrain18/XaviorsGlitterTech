using HarmonyLib;
using Verse;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    internal static class GlitterTechPatcher
    {
        static GlitterTechPatcher()
        {
            var harmony = new Harmony("rimworld.xavior.glittertech");
            harmony.PatchAll();
        }
    }
}
