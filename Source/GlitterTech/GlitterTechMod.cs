using HarmonyLib;
using UnityEngine;
using Verse;

namespace GlitterTech
{
    // Main class for the GlitterTech mod
    class GlitterTechMod : Mod
    {
        public static GlitterTechSettings settings;

        public GlitterTechMod(ModContentPack content) : base(content)
        {
            // Initializing Harmony with a unique ID
            var harmony = new Harmony("com.xavior.glittertech");
            harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            // Load settings
            settings = GetSettings<GlitterTechSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            // Creating a slider in the settings window
            ls.Label($"Resource Stat Multiplier: {settings.statMultiplier:P0} (default is 100%)");
            settings.statMultiplier = ls.Slider(settings.statMultiplier, 0.01f, 2.00f);

            ls.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "GlitterTech";
        }
    }
}
