using HarmonyLib;
using UnityEngine;
using Verse;
using System.Linq;

namespace GlitterTech
{
    public class GlitterTechMod : Mod
    {
        public static GlitterTechSettings settings;

        public GlitterTechMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<GlitterTechSettings>();
            ApplyPatches();
            ApplyNoSurgerySettings();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            ls.Label("⚠️ This setting should only be changed before starting a new game.");
            ls.CheckboxLabeled("Activate No-Surgery Version", ref settings.activateNoSurgeryVersion);

            ls.Gap(24f);
            ls.Label("Incident Settings");
            ls.GapLine();

            ls.CheckboxLabeled("Enable Three-Way Battle Incident", ref settings.enableThreeWayBattle, "Toggle the three-way battle incident on or off.");
            ls.Label($"Wealth Threshold: {settings.wealthThreshold:0}");
            settings.wealthThreshold = ls.Slider(settings.wealthThreshold, 5000f, 100000f);
            ls.Label($"Points Multiplier: {settings.pointsMultiplier:0.00}");
            settings.pointsMultiplier = ls.Slider(settings.pointsMultiplier, 0.1f, 3.0f);

            ls.Gap(24f);
            ls.Label("Resource Settings");
            ls.GapLine();
            ls.Label($"Resource Yield Multiplier: {settings.resourceMultiplier:0.00}");
            settings.resourceMultiplier = ls.Slider(settings.resourceMultiplier, 0.01f, 2.00f);

            ls.Label("Stat Bases Settings");
            ls.GapLine();
            ls.Label($"Stat Multiplier: {settings.statMultiplier:0.00}");
            settings.statMultiplier = ls.Slider(settings.statMultiplier, 0.01f, 2.00f);

            if (ls.ButtonText("Apply Changes"))
            {
                ApplyPatches();
            }

            ls.End();
            base.DoSettingsWindowContents(inRect);
        }

        private static void ApplyPatches()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.defName == "Titanium_GT" || def.defName == "AlphaPoly" || def.defName == "BetaPoly")
                {
                    StatBasesPatch.ModifySpecificStats(def, settings.statMultiplier);
                }

                if (def.defName == "MineableTitanium")
                {
                    ResourceYieldPatch.SaveOriginalValues(def);
                    ResourceYieldPatch.ModifyYield(def.building, settings.resourceMultiplier);
                }
            }
        }

        private static void ApplyNoSurgerySettings()
        {
            if (settings.activateNoSurgeryVersion)
            {
                // Remove recipes from benches / surgery tables
                foreach (var thing in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (thing.recipes != null)
                    {
                        thing.recipes.RemoveAll(recipe =>
                            recipe.defName.StartsWith("InstallGT") || recipe.defName.StartsWith("InstallOC"));
                    }
                }

                // Remove Hediffs
                DefDatabase<HediffDef>.AllDefsListForReading.RemoveAll(hediff =>
                    hediff.defName.StartsWith("GT") || hediff.defName.StartsWith("OC"));

                // Remove ThingDefs
                DefDatabase<ThingDef>.AllDefsListForReading.RemoveAll(thing =>
                    thing.defName.StartsWith("GT") || thing.defName.StartsWith("OC"));
            }
        }


        public override string SettingsCategory()
        {
            return "GlitterTech";
        }
    }
}
