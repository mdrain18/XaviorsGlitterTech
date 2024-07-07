using HarmonyLib;
using UnityEngine;
using Verse;

namespace GlitterTech
{
    public class GlitterTechMod : Mod
    {
        public static GlitterTechSettings settings;

        public GlitterTechMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<GlitterTechSettings>();
            ApplyPatches();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            // Incident settings
            ls.Label("Incident Settings");
            ls.GapLine();

            ls.CheckboxLabeled("Enable Three-Way Battle Incident", ref settings.enableThreeWayBattle, "Toggle the three-way battle incident on or off.");
            TooltipHandler.TipRegion(ls.GetRect(24f), "Enable or disable the three-way battle incident.");

            ls.Label($"Wealth Threshold: {settings.wealthThreshold:0} (default is 50000)");
            settings.wealthThreshold = ls.Slider(settings.wealthThreshold, 5000f, 100000f);
            TooltipHandler.TipRegion(ls.GetRect(24f), "Set the wealth threshold required to trigger the incident.");

            ls.Label($"Points Multiplier: {settings.pointsMultiplier:0.00} (default is 0.75)");
            settings.pointsMultiplier = ls.Slider(settings.pointsMultiplier, 0.1f, 3.0f);
            TooltipHandler.TipRegion(ls.GetRect(24f), "Multiplier for the points used to generate raid groups in the incident.");

            ls.Gap(24f); // Add some gap between the sections

            // Resource settings
            ls.Label("Resource Settings");
            ls.GapLine();

            ls.Label($"Resource Yield Multiplier: {settings.resourceMultiplier:0.00} (default is 1.00)");
            settings.resourceMultiplier = ls.Slider(settings.resourceMultiplier, 0.01f, 2.00f);
            TooltipHandler.TipRegion(ls.GetRect(24f), "Multiplier for the yield of titanium when mined.");

            // Stat bases settings
            ls.Label("Stat Bases Settings");
            ls.GapLine();

            ls.Label($"Stat Multiplier: {settings.statMultiplier:0.00} (default is 1.00)");
            settings.statMultiplier = ls.Slider(settings.statMultiplier, 0.01f, 2.00f);
            TooltipHandler.TipRegion(ls.GetRect(24f), "Multiplier for the stat bases of titanium, alpha poly, and beta poly.");

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

        public override string SettingsCategory()
        {
            return "GlitterTech";
        }
    }
}
