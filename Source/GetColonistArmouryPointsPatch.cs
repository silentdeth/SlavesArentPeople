using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SlavesArentPeople.CombatReadinessCheckCompat
{
    public static class GetColonistArmouryPointsPatch
    {
        public static bool Prefix(
        IEnumerable<Pawn> colonists,
        IIncidentTarget target,
        out float colonistPoints,
        out float caravanArmouryWealth)
        {
            var num1 = 0.0f;
            var num2 = 0.0f;
            var num3 = 1f;
            var num4 = 1f;
            if (target is Caravan)
            {
                num3 = 0.7f;
                num4 = 0.5f;
            }
            
            var GetArmouryWealth = Traverse.Create(typeof(CRC_Reintegrated.ArmouryUtility)).Method("GetArmouryWealth", new Type[] { typeof(Pawn) });
            var GetBattleScore = Traverse.Create(typeof(CRC_Reintegrated.ArmouryUtility)).Method("GetBattleScore", new Type[] { typeof(Pawn) });
            foreach (var colonist in colonists)
            {
                if (colonist.ParentHolder != null && (colonist.ParentHolder is Building_CryptosleepCasket || colonist.ParentHolder is CompBiosculpterPod))
                {
                    if (SlavesArentPeople.SAP_Settings.debugLog)
                        Debug.Log("Slaves Aren't People: GetColonistArmouryPointsPatch: skipping colonist because cascket or biosculpter: " + colonist.Name);
                    continue;
                }
                //if treated as prisoners slaves have no effect
                if(SlavesArentPeople.SAP_Settings.slavesArePrisoners && colonist.IsSlaveOfColony)
                {
                    if (SlavesArentPeople.SAP_Settings.debugLog)
                        Debug.Log("Slaves Aren't People: GetColonistArmouryPointsPatch: skipping slave(prisoner): " + colonist.Name);

                    continue;
                }
                if (target is Caravan)
                {
                    if (SlavesArentPeople.SAP_Settings.slavesAreFurniture && colonist.IsSlaveOfColony)
                        num2 += (float)((float)GetArmouryWealth.GetValue(colonist) 
                            * num4 
                            * (double)SlavesArentPeople.CRC_Compat.percentOfValueForBuildings / 100.0);
                    else
                        num2 += (float)GetArmouryWealth.GetValue(colonist) * num4;
                }

                if (colonist.RaceProps.Animal)
                {
                    if (colonist.Faction == Faction.OfPlayer && !colonist.Downed &&
                        colonist.training.HasLearned(TrainableDefOf.Release) &&
                        colonist.health.capacities.GetLevel(PawnCapacityDefOf.Moving) >= 0.15)
                    {
                        num1 += (float)(SlavesArentPeople.CRC_Compat.percentOfCombatPowerForReleasableAnimals *
                            (double)colonist.kindDef.combatPower / 100.0) * num3;
                    }
                }
                else if (!colonist.WorkTagIsDisabled(WorkTags.Violent) && colonist.health.capacities.GetLevel(PawnCapacityDefOf.Moving) >= 0.15)
                {
                    if (colonist.IsFreeNonSlaveColonist)
                        num1 += (float)GetBattleScore.GetValue(colonist);
                    else if (colonist.IsSlaveOfColony)
                    {
                        /*
                        if (SlavesArentPeople.SAP_Settings.slavesAreFurniture)
                            num1 += (float)((float)GetBattleScore.GetValue(colonist)
                                * (double)SlavesArentPeople.CRC_Compat.percentOfValueForBuildings / 100.0);
                        */

                        //if furniture, points only based on wealth
                        //in prisoners, slaves have no effect on points. 
                        //if animals, points based on combat power.
                        if (SlavesArentPeople.SAP_Settings.slavesAreAnimals)
                            num1 += (float)(SlavesArentPeople.CRC_Compat.percentOfCombatPowerForReleasableAnimals *
                            (double)colonist.kindDef.combatPower / 100.0) * num3;
                    }
                }
            }

            if (target is Caravan)
            {
                num2 /= 2f;
            }

            caravanArmouryWealth = num2;
            colonistPoints = num1;

            if (SlavesArentPeople.SAP_Settings.debugLog)
                Debug.Log("Slaves Aren't People: GetColonistArmouryPointsPatch: colonistPoints: " + colonistPoints + " caravanArmouryWealth: " + caravanArmouryWealth);

            return false;
        }
    }
}
