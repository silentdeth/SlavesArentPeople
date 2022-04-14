using HarmonyLib;
using HugsLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SlavesArentPeople.PrepareForCombatCompat
{
    public static class PFC_DefaultThreatPointsNow_Patch
    {   [HarmonyBefore(new string[] { "UnlimitedHugs.HugsLib" }) ]
        public static bool Prefix(Map __instance, IIncidentTarget target, ref float __result)
        {
            float wealthForStoryteller = target.PlayerWealthForStoryteller;
            var PointsPerWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof(StorytellerUtility), "PointsPerWealthCurve");
            var PointsPerColonistByWealthCurve = AccessTools.StaticFieldRefAccess<SimpleCurve>(typeof(StorytellerUtility), "PointsPerColonistByWealthCurve");
            float num1 = PointsPerWealthCurve.Evaluate(wealthForStoryteller);
            float num2 = 0.0f;
            foreach (Pawn p in target.PlayerPawnsForStoryteller)
            {
                if ((!PrepareForCombat.PrepareForCombat.Instance.combatCapablePawns || p.ParentHolder == null || !((p.ParentHolder is Building_CryptosleepCasket) || (p.ParentHolder is CompBiosculpterPod))) && !p.IsQuestLodger())
                {
                    float a = 0.0f;
                    if (p.IsFreeColonist && PrepareForCombat.PrepareForCombat.Instance.combatCapablePawns || p.IsFreeColonist && PrepareForCombat.PrepareForCombat.Instance.combatCapablePawns && !p.WorkTagIsDisabled(WorkTags.Violent) && (double)p.health.capacities.GetLevel(PawnCapacityDefOf.Moving) >= 0.15)
                        a = PointsPerColonistByWealthCurve.Evaluate(wealthForStoryteller);
                    else if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && !p.Downed && p.training.CanAssignToTrain(TrainableDefOf.Release).Accepted && !PrepareForCombat.PrepareForCombat.Instance.combatCapablePawns || p.RaceProps.Animal && p.Faction == Faction.OfPlayer && !p.Downed && p.training.HasLearned(TrainableDefOf.Release) && PrepareForCombat.PrepareForCombat.Instance.combatCapablePawns && (double)p.health.capacities.GetLevel(PawnCapacityDefOf.Moving) >= 0.15)
                    {
                        a = 0.08f * p.kindDef.combatPower * PrepareForCombat.PrepareForCombat.Instance.percentPowerAnimals;
                        if (target is Caravan)
                            a *= 0.7f;
                    }
                    if ((double)a > 0.0)
                    {
                        //add biosclupter to check
                        if (p.ParentHolder != null && (p.ParentHolder is Building_CryptosleepCasket || p.ParentHolder is CompBiosculpterPod))
                            a *= 0.3f;
                        //adjust raid points for slaves
                        if (p.IsSlaveOfColony)
                        {
                            if (SlavesArentPeople.SAP_Settings.debugLog)
                            {
                                Debug.Log("Slaves Aren't People: PFC_DefaultThreatPointsNowPatch: Found Slave: " + p.Name);
                                Debug.Log("Slaves Aren't People: PFC_DefaultThreatPointsNowPatch: Changing raid points from: " + a +
                                    " to: " + a * (float)SlavesArentPeople.SAP_Settings.raidPointsSlaves / 100f);
                            }
                            
                            a *= (float)SlavesArentPeople.SAP_Settings.raidPointsSlaves / 100f;
                        }
                        float num3 = Mathf.Lerp(a, a * p.health.summaryHealth.SummaryHealthPercent, 0.65f);
                        num2 += num3;
                    }
                }
            }
            float num4 = (num1 + num2) * Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor) * target.IncidentPointsRandomFactorRange.RandomInRange * Find.Storyteller.difficulty.threatScale * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassed);
            __result = Mathf.Clamp(num4, 35f, 10000f);
            return false;
        }
    }
}
