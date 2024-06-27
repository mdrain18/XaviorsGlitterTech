using System.Collections.Generic;
using GlitterTech;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

public class IncidentWorker_ThreeWayBattle : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        return GlitterTechMod.settings.enableThreeWayBattle && map.wealthWatcher.WealthTotal >= GlitterTechMod.settings.wealthThreshold;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!CanFireNowSub(parms))
        {
            Log.Error("Incident cannot fire now.");
            return false;
        }

        // Generate factions
        Faction orionFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("OrionCo"));
        Faction commandoFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Commandos"));
        Faction empireFaction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Empire"));

        if (orionFaction == null || commandoFaction == null || empireFaction == null)
        {
            Log.Error("One or more factions are null.");
            return false;
        }

        // Generate pawn groups
        List<Pawn> orionGroup = GeneratePawns(orionFaction, map, out IncidentParms orionParms, 0.40f * GlitterTechMod.settings.pointsMultiplier);
        List<Pawn> commandoGroup = GeneratePawns(commandoFaction, map, out IncidentParms commandoParms, 0.40f * GlitterTechMod.settings.pointsMultiplier);
        List<Pawn> empireGroup = GeneratePawns(empireFaction, map, out IncidentParms empireParms, 2.0f * GlitterTechMod.settings.pointsMultiplier);

        if (orionGroup == null || orionGroup.Count == 0 || commandoGroup == null || commandoGroup.Count == 0 || empireGroup == null || empireGroup.Count == 0)
        {
            Log.Error("One or more pawn groups are null or empty.");
            return false;
        }

        // Define drop pod spots near the center
        orionParms.spawnCenter = GetDropSpotNearCenter(map);
        commandoParms.spawnCenter = GetDropSpotNearCenter(map);
        empireParms.spawnCenter = GetDropSpotNearCenter(map);

        if (!orionParms.spawnCenter.IsValid || !commandoParms.spawnCenter.IsValid || !empireParms.spawnCenter.IsValid)
        {
            Log.Error("One or more drop pod spots are invalid.");
            return false;
        }

        // Set drop pod arrival mode
        orionParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
        commandoParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
        empireParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;

        // Create lords for the battle
        Lord orionLord = CreateLordForBattle(orionFaction, map, orionGroup);
        Lord commandoLord = CreateLordForBattle(commandoFaction, map, commandoGroup);
        Lord empireLord = CreateLordForBattle(empireFaction, map, empireGroup);

        // Drop the groups near the center
        DropPodUtility.DropThingsNear(orionParms.spawnCenter, map, orionGroup, 110, false, true, true);
        DropPodUtility.DropThingsNear(commandoParms.spawnCenter, map, commandoGroup, 110, false, true, true);
        DropPodUtility.DropThingsNear(empireParms.spawnCenter, map, empireGroup, 110, false, true, true);

        Messages.Message("A high tech battle has started near your base!", MessageTypeDefOf.ThreatBig, true);

        // Add a watcher to monitor the battle
        MapComponent_ThreeWayBattleWatcher watcher = map.GetComponent<MapComponent_ThreeWayBattleWatcher>();
        if (watcher == null)
        {
            watcher = new MapComponent_ThreeWayBattleWatcher(map);
            map.components.Add(watcher);
        }
        watcher.StartWatching(orionLord, commandoLord, empireLord);

        return true;
    }

    private IntVec3 GetDropSpotNearCenter(Map map)
    {
        IntVec3 result = IntVec3.Invalid;
        CellFinder.TryFindRandomCellNear(map.Center, map, 20, (IntVec3 c) => c.Standable(map) && !c.Roofed(map), out result);
        return result;
    }

    private List<Pawn> GeneratePawns(Faction faction, Map map, out IncidentParms incidentParms, float adjustedPointsMultiplier)
    {
        incidentParms = new IncidentParms
        {
            target = map,
            faction = faction,
            points = StorytellerUtility.DefaultThreatPointsNow(map) * adjustedPointsMultiplier,
            pawnGroupMakerSeed = Rand.Int
        };
        incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop; // Ensure raidArrivalMode is not null
        incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack; // Ensure raidStrategy is not null
        PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
        pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(pawnGroupMakerParms.points, incidentParms.raidArrivalMode, incidentParms.raidStrategy, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, null);
        return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
    }

    private Lord CreateLordForBattle(Faction faction, Map map, List<Pawn> pawns)
    {
        LordJob lordJob = new LordJob_AssaultColony(faction);
        Lord lord = LordMaker.MakeNewLord(faction, lordJob, map, pawns);
        return lord;
    }
}

// Custom MapComponent to watch and handle the battle
public class MapComponent_ThreeWayBattleWatcher : MapComponent
{
    private Lord orionLord;
    private Lord commandoLord;
    private Lord empireLord;
    private bool watching;

    public MapComponent_ThreeWayBattleWatcher(Map map) : base(map)
    {
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (watching)
        {
            if (CheckBattleEnded())
            {
                EndBattle();
            }
        }
    }

    public void StartWatching(Lord orion, Lord commando, Lord empire)
    {
        orionLord = orion;
        commandoLord = commando;
        empireLord = empire;
        watching = true;
    }

    private bool CheckBattleEnded()
    {
        return orionLord.ownedPawns.All(p => p.Dead || p.Downed) ||
               commandoLord.ownedPawns.All(p => p.Dead || p.Downed) ||
               empireLord.ownedPawns.All(p => p.Dead || p.Downed);
    }

    private void EndBattle()
    {
        watching = false;
        AssignLeaveJob(orionLord);
        AssignLeaveJob(commandoLord);
        AssignLeaveJob(empireLord);
    }

    private void AssignLeaveJob(Lord lord)
    {
        if (lord != null && lord.ownedPawns != null)
        {
            lord.ReceiveMemo("BattleEnded");
            foreach (Pawn pawn in lord.ownedPawns)
            {
                if (pawn.Dead || pawn.Downed) continue;
                pawn.GetLord().ownedPawns.Remove(pawn);
                Lord newLord = LordMaker.MakeNewLord(lord.faction, new LordJob_ExitMapBest(LocomotionUrgency.Walk, true), map, new List<Pawn> { pawn });
                newLord.AddPawn(pawn);
            }
        }
    }
}
