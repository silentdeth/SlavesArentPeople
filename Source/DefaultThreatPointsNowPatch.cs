using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using static SlavesArentPeople.SlavesArentPeople;

namespace SlavesArentPeople
{
    public static class DefaultThreatPointsNowPatch
    {
        //[HarmonyAfter(new string[] { "net.marvinkosh.rimworld.mod.combatreadinesscheck" })]
        public static float Postfix(float __result, Map __instance, IIncidentTarget target)
        {
            /*
            if (ModLister.GetActiveModWithIdentifier("Mlie.CombatReadinessCheck") != null)
            {
                if (SlavesArentPeople.SAP_Settings.debugLog)
                    Debug.Log("Slaves Aren't People: Postfix: Combat Readiness Check Detected");
                return patchCombatReadinessCheckCompat(__result, target, ref __instance);
            }
            else if (ModLister.GetActiveModWithIdentifier("Bar0th.PFC") != null)
            {
                if (SlavesArentPeople.SAP_Settings.debugLog)
                    Debug.Log("Slaves Aren't People: Postfix: Prepare for Combat Detected");
                return patchVanilla(__result, target);
            }
            else*/
            if(!(SlavesArentPeople.SAP_Settings.hasCRC ||
                SlavesArentPeople.SAP_Settings.hasPFC))
            {
                if (SlavesArentPeople.SAP_Settings.debugLog)
                    Debug.Log("Slaves Aren't People: Postfix: Using vanilla patch");
                return patchVanilla(__result, target);
            }
            return __result;
        }

        /***********************************************************************
         * Patch for vanilla Map.DefaultThreatPointsNow
         ***********************************************************************/
        public static float patchVanilla(float __result, IIncidentTarget target)
        {
            if (SlavesArentPeople.SAP_Settings.debugLog)
                Debug.Log("Slaves Aren't People: Vanilla DefaultThreatPointsNowPatch: Old raid points: " + __result);

            if (__result == 35 || __result == 10000)
                return __result;
            float wealthForStoryteller = target.PlayerWealthForStoryteller;

            //Reverse raid point calculation to get base value from colonist
            var basePoints = __result;
            var PointsPerWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof(StorytellerUtility), "PointsPerWealthCurve");
            var PointsPerColonistByWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof(StorytellerUtility), "PointsPerColonistByWealthCurve");
            basePoints /= target.IncidentPointsRandomFactorRange.RandomInRange *
                        Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor)
                            * Find.Storyteller.difficulty.threatScale
                            * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassedSinceSettle);
            basePoints -= PointsPerWealthCurve.Evaluate(wealthForStoryteller);

            if (SlavesArentPeople.SAP_Settings.debugLog)
            {
                Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: Old colonist points: " + basePoints);

                //Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: Old wealth: " + wealthForStoryteller);
            }
            /**********************************
            ****MOVED TO ForceRecountPatch*****
            ***********************************
            //remove the wealth added by slaves
            foreach (Pawn p in target.PlayerPawnsForStoryteller)
            {
                if (p.IsFreeColonist && p.IsSlaveOfColony)
                {
                    //if animals increase wealth from 75% to 100%
                    if (SAP_Settings.slavesAreAnimals)
                        wealthForStoryteller += p.MarketValue * 0.25f;
                    //if furniture reduce wealth to 50% from 75% and equipment to 50% from 100%
                    if (SAP_Settings.slavesAreFurniture)
                        wealthForStoryteller -= p.MarketValue * 0.25f
                            + WealthWatcher.GetEquipmentApparelAndInventoryWealth(p) * 0.50f;
                    //if prisoners reduce wealth to 0% from 75% and equipment to 0% from 100%
                    if (SAP_Settings.slavesArePrisoners)
                        wealthForStoryteller -= p.MarketValue * 0.75f
                            + WealthWatcher.GetEquipmentApparelAndInventoryWealth(p);
                }
            }
            if (SlavesArentPeople.SAP_Settings.debugLog)
                Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: New wealth: " + wealthForStoryteller);
            */

            //remove the raid points added by slaves
            foreach (Pawn p in target.PlayerPawnsForStoryteller)
            {
                if (p.IsFreeColonist)
                {
                    //if not a slave, no changes needed
                    if (!p.IsSlaveOfColony)
                    {
                        continue;
                    }
                    if (SlavesArentPeople.SAP_Settings.debugLog && p.IsSlaveOfColony)
                        Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: Found Slave: " + p.Name);
                    //recalculate points per colonist based on new wealth value
                    float a = PointsPerColonistByWealthCurve.Evaluate(wealthForStoryteller);
                    if (p.ParentHolder != null && (p.ParentHolder is Building_CryptosleepCasket || p.ParentHolder is CompBiosculpterPod))
                        a *= 0.3f;
                    float num3 = Mathf.Lerp(a, a * p.health.summaryHealth.SummaryHealthPercent, 0.65f);
                    if (SlavesArentPeople.SAP_Settings.debugLog && p.IsSlaveOfColony)
                        Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: Slave " + p.Name + " Old Value: " + num3);
                    //if animals, points are 8% of combat power
                    if (SAP_Settings.slavesAreAnimals && p.IsSlaveOfColony)
                    {
                        num3 *= 0.75f;
                        basePoints -= num3;
                        a = 0.08f * p.kindDef.combatPower;
                        //a is reset above, so redo Cryptosleep/Biosculpt check
                        if (p.ParentHolder != null && (p.ParentHolder is Building_CryptosleepCasket || p.ParentHolder is CompBiosculpterPod))
                            a *= 0.3f;
                        num3 = Mathf.Lerp(a, a * p.health.summaryHealth.SummaryHealthPercent, 0.65f);
                    }
                    //if Furniture, points are based solely on wealth
                    //if prisoners, slaves have no effect on points
                    else if ((SAP_Settings.slavesAreFurniture || SAP_Settings.slavesArePrisoners) && p.IsSlaveOfColony)
                    {
                        num3 *= 0.75f;
                        basePoints -= num3;
                        num3 = 0; // for debug message
                    }
                    
                    if (SlavesArentPeople.SAP_Settings.debugLog && p.IsSlaveOfColony)
                        Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: Slave " + p.Name + " New Value: " + num3);
                    basePoints += num3;

                }
            }
            if (SlavesArentPeople.SAP_Settings.debugLog)
            {
                Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: New colonist points: " + basePoints);

                Debug.Log("Slaves Aren't People: DefaultThreatPointsNowPatch: New raid points: " + Mathf.Clamp((basePoints + PointsPerWealthCurve.Evaluate(wealthForStoryteller))
                    * target.IncidentPointsRandomFactorRange.RandomInRange
                    * Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor)
                    * Find.Storyteller.difficulty.threatScale
                    * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassedSinceSettle), 35f, 10000f)
                    );
            }
            return Mathf.Clamp((basePoints + PointsPerWealthCurve.Evaluate(wealthForStoryteller))
                * target.IncidentPointsRandomFactorRange.RandomInRange
                * Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor)
                * Find.Storyteller.difficulty.threatScale
                * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassedSinceSettle), 35f, 10000f);


        }
    }
}
