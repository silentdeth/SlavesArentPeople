﻿using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SlavesArentPeople
{
    public class SlavesArentPeople : Mod
    {
        /*pretranslate text
        private const string ModName = "Slaves Aren't People";
        private const string SettingFurniture = "Slaves are furniture";
        private const string SettingFurnitureTooltip = "Slaves and thier equipment count towards colony wealth at half value.";
        private const string SettingPrisoners = "Slaves are prisoners";
        private const string SettingPrisonersTooltip = "Slaves have no effect on raid points.";
        private const string SettingAnimals = "Slaves are animals";
        private const string SettingAnimalsTooltip = "Slaves and thier equipment count towards colony wealth. Additional points are added based on the slave's combat potential.";
        private const string SettingDebug = "Debug Logging";
        */
        public static SAP_Settings settings;
        public static CRC_Compatibility CRC_Compat;
        private static Harmony harmony;

        private static string currentVersion;

        public SlavesArentPeople(ModContentPack content) : base(content)
        {
            harmony = new Harmony("net.rimworld.mod.slavesarentpeople");
            currentVersion = "1.0";
            settings = GetSettings<SAP_Settings>();
            if (ModLister.GetActiveModWithIdentifier("Mlie.CombatReadinessCheck") != null)
            {
                CRC_Compat = new CRC_Compatibility();
                CRC_Map_PlayerWealthForStoryteller_Patch();
                CRC_GetColonistArmouryPoints_Patch();
            }
            else
            {
                CRC_Compat = null;
                PatchRaidPoints();
            }

            
        }

        private static void PatchRaidPoints()
        {
            Log.Message("Slaves Aren't People: Trying to patch StorytellerUtility.DefaultThreatPointsNow");
            var origMethod = AccessTools.Method(typeof(StorytellerUtility), nameof(StorytellerUtility.DefaultThreatPointsNow));
            if (origMethod == null)
            {
                Log.Warning(
                    "Got null original method when attempting to find original StorytellerUtility.DefaultThreatPointsNow.");
            }
            else
            {
                var moddedMethod = AccessTools.Method(typeof(DefaultThreatPointsNowPatch), "Postfix");
                if (moddedMethod == null)
                {
                    Log.Warning("Got null method when attempting to load DefaultThreatPointsNowPatch.Postfix.");
                }
                else
                {
                    harmony.Patch(origMethod, null, new HarmonyMethod(moddedMethod));
                    Log.Message("Slaves Aren't People: Patched StorytellerUtility.DefaultThreatPointsNow.");
                }
            }

        }
        
        private static void CRC_Map_PlayerWealthForStoryteller_Patch()
        {
            Log.Message("Slaves Aren't People: Trying to patch Map.PlayerWealthForStoryteller");
            var getMethod = AccessTools.Method(typeof(Map), "get_PlayerWealthForStoryteller");
            if (getMethod == null)
            {
                Log.Warning(
                    "Got null original method when attempting to find original Map.PlayerWealthForStoryteller.");
            }
            else
            {
                var method = AccessTools.Method(typeof(CombatReadinessCheckCompat.MapWealthForStoryTellerPatch), "Postfix");
                if (method == null)
                {
                    Log.Warning("Got null method when attempting to load postfix.");
                }
                else
                {
                    harmony.Patch(getMethod, null, new HarmonyMethod(method));
                    Log.Message("Slaves Aren't People: Patched Map.PlayerWealthForStoryteller.");
                }
            }
        }

        private static void CRC_GetColonistArmouryPoints_Patch()
        {

            Log.Message("Slaves Aren't People: Trying to patch ArmouryUtility.GetColonistArmouryPoints");
            var origMethod = AccessTools.Method(typeof(CRC_Reintegrated.ArmouryUtility), "GetColonistArmouryPoints");
            if (origMethod == null)
            {
                Log.Warning(
                    "Got null original method when attempting to find original ArmouryUtility.GetColonistArmouryPoints.");
            }
            else
            {
                var method = AccessTools.Method(typeof(CombatReadinessCheckCompat.GetColonistArmouryPointsPatch), "Prefix");
                if (method == null)
                {
                    Log.Warning("Got null method when attempting to load Prefix.");
                }
                else
                {
                    harmony.Patch(origMethod, new HarmonyMethod(method));
                    Log.Message("Slaves Aren't People: Patched ArmouryUtility.GetColonistArmouryPoints.");
                }
            }
        }

        public override string SettingsCategory()
        {
            return "SlavesArentPeople_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard1 = new Listing_Standard
            {
                ColumnWidth = inRect.width / 3f
            };
            //var listingStandard1 = listingStandard1;
            listingStandard1.Begin(inRect);
            listingStandard1.Gap();
            listingStandard1.CheckboxLabeled("SlavesArentPeople_SettingFurniture".Translate(), ref SAP_Settings.slavesAreFurniture, "SlavesArentPeople_SettingFurnitureTooltip".Translate());
            listingStandard1.CheckboxLabeled("SlavesArentPeople_SettingPrisoners".Translate(), ref SAP_Settings.slavesArePrisoners, "SlavesArentPeople_SettingPrisonersTooltip".Translate());
            listingStandard1.CheckboxLabeled("SlavesArentPeople_SettingAnimals".Translate(), ref SAP_Settings.slavesAreAnimals, "SlavesArentPeople_SettingAnimalsTooltip".Translate());
            listingStandard1.Gap();
            listingStandard1.CheckboxLabeled("SlavesArentPeople_SettingDebug".Translate(), ref SAP_Settings.debugLog);
            listingStandard1.End();

        }

        public class SAP_Settings : ModSettings
        {
            public static bool slavesAreFurniture = true;
            public static bool slavesArePrisoners;
            public static bool slavesAreAnimals;
            
            public static bool debugLog = true;

            public override void ExposeData()
            {
                Scribe_Values.Look(ref slavesAreFurniture, "slavesAreFurniture", true);
                Scribe_Values.Look(ref slavesArePrisoners, "slavesArePrisoners");
                Scribe_Values.Look(ref slavesAreAnimals, "slavesAreAnimals");
                Scribe_Values.Look(ref debugLog, "debugLog", true);
                base.ExposeData();
            }
        }

        public class CRC_Compatibility
        {
            public int numPointsPerColonist = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<int>("numPointsPerColonist").Value;

            public float percentOfCombatPowerForReleasableAnimals = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<float>("percentOfCombatPowerForReleasableAnimals").Value;

            public float percentOfValueForArmour = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<float>("percentOfValueForArmour").Value;

            public float percentOfValueForBuildings = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<float>("percentOfValueForBuildings").Value;

            public float percentOfValueForIndustrialWeapons = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<float>("percentOfValueForIndustrialWeapons").Value;

            public float percentOfValueForPreIndustrialWeapons = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<float>("percentOfValueForPreIndustrialWeapons").Value;

            public bool preIndustrialArmor = Traverse.Create(AccessTools.TypeByName("CRC_Loader").GetField("settings", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Field<bool>("preIndustrialArmor").Value;
         
        }
    }

}