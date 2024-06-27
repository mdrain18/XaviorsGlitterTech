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

            // Checkbox for enabling/disabling the incident
            ls.CheckboxLabeled("Enable Three-Way Battle Incident", ref settings.enableThreeWayBattle, "Toggle the three-way battle incident on or off.");

            // Slider for the wealth threshold
            ls.Label($"Wealth Threshold: {settings.wealthThreshold:0} (default is 50000)");
            settings.wealthThreshold = ls.Slider(settings.wealthThreshold, 5000f, 100000f);

            // Slider for the points multiplier
            ls.Label($"Points Multiplier: {settings.pointsMultiplier:0.00} (default is 0.75)");
            settings.pointsMultiplier = ls.Slider(settings.pointsMultiplier, 0.1f, 3.0f);

            // Slider for the resource stat multiplier
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
