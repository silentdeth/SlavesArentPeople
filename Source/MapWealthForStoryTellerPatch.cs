using CRC_Reintegrated;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SlavesArentPeople.CombatReadinessCheckCompat
{
    public static class MapWealthForStoryTellerPatch
    {
        public static float Postfix(float __result, Map __instance)
        {
            //only need to alter the result if using furniture setting
            if (!SlavesArentPeople.SAP_Settings.slavesAreFurniture)
                return __result;

            var num1 = 0.0f;
            var num2 = 0.0f;
            if (__instance.IsPlayerHome)
            {
                //wealth of unequiped stuff
                ArmouryUtility.GetStorytellerArmouryPoints(__instance, out var armouryPoints);

                num1 = MarvsStoryTellerUtility.PointsPerWealthCurve.Evaluate(armouryPoints);

                var x = (float)(__instance.wealthWatcher.WealthBuildings *
                (double)SlavesArentPeople.CRC_Compat.percentOfValueForBuildings / 100.0);

                //if furniture, pawns count at half value
                if (SlavesArentPeople.SAP_Settings.slavesAreFurniture)
                {
                    if (SlavesArentPeople.SAP_Settings.debugLog)
                        Debug.Log("Slaves Aren't People: CRC MapWealthForStoryTellerPatch: wealth before pawns: " + x);

                    foreach (Pawn p in __instance.PlayerPawnsForStoryteller)
                    {
                        if (p.IsFreeColonist && p.IsSlaveOfColony)
                        {
                            x += (float)(p.MarketValue * (double)SlavesArentPeople.CRC_Compat.percentOfValueForBuildings / 100.0);
                        }
                    }

                    if (SlavesArentPeople.SAP_Settings.debugLog)
                        Debug.Log("Slaves Aren't People: CRC MapWealthForStoryTellerPatch: wealth after pawns: " + x);
                }

                num2 = MarvsStoryTellerUtility.PointsPerWealthCurve.Evaluate(x);

                __result = num1 + num2;

            }
            else
            {
                //does this ever run? Maybe when attacking other factions places
                //yes that must be it.
                if (SlavesArentPeople.SAP_Settings.debugLog)
                    Debug.Log("Slaves Aren't People: CRC MapWealthForStoryTellerPatch: is map and not IsPlayerHome");
                __result = 0.0f;
                /*
                * if we made it to here 
                * SlavesArentPeople.SAP_Settings.slavesAreFurniture = true
                */
                foreach (Pawn p in __instance.mapPawns.PawnsInFaction(Faction.OfPlayer))
                {
                    if (p.IsFreeColonist && p.IsSlaveOfColony)
                    {
                        __result += p.MarketValue;
                    }
                }
                __result *= (float)SlavesArentPeople.CRC_Compat.percentOfValueForBuildings / 100.0f;
            }

            return __result;
        }
    }
}
