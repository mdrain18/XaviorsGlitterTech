using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GlitterTech
{
    [StaticConstructorOnStartup]
    public static class DrillSignatureDumper
    {
        static DrillSignatureDumper()
        {
            foreach (var m in typeof(DeepDrillUtility).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                //Log.Warning($"[GlitterTech] Method: {m.Name} with {m.GetParameters().Length} parameters: {string.Join(", ", m.GetParameters().Select(p => p.ParameterType.ToString()))}");
            }
        }
    }
}
