using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SlavesArentPeople
{
    public static class ForceRecountPatch
    {
        public static void Postfix(WealthWatcher __instance, bool allowDuringInit)
        {
            if (!allowDuringInit && Current.ProgramState != ProgramState.Playing)
            {
                return;
            }
            float wealthPawns = (float)Traverse.Create(__instance).Field("wealthPawns").GetValue();
            if (SlavesArentPeople.SAP_Settings.debugLog)
                Debug.Log("Slaves Aren't People: ForceRecountPatch: Starting wealthPawns: " + wealthPawns);
            foreach (Pawn p in ((Map)Traverse.Create(__instance).Field("map").GetValue()).mapPawns.PawnsInFaction(Faction.OfPlayer))
            {
                if (!p.IsQuestLodger())
                {
                    float marketValue = p.MarketValue;
                    if (p.IsSlave)
                    {
                        if (SlavesArentPeople.SAP_Settings.debugLog)
                            Debug.Log("Slaves Aren't People: ForceRecountPatch: Adjusting MarketValue of slave: " + p.Name + " from: " + (marketValue * 0.75f));

                        if (SlavesArentPeople.SAP_Settings.hasPFC)
                            marketValue *= (float)SlavesArentPeople.SAP_Settings.wealthSlaves / 100f - 0.75f;
                        else if (SlavesArentPeople.SAP_Settings.slavesAreAnimals)
                            marketValue *= 0.25f; //increase from 75% to 100%
                        else if (SlavesArentPeople.SAP_Settings.slavesArePrisoners)
                            marketValue *= -0.75f; //decrease from 75% to 0%
                        else
                            marketValue *= -0.25f; //decrease from 75% to 50%

                        wealthPawns += marketValue; //adjusted wealth

                        if (SlavesArentPeople.SAP_Settings.debugLog)
                            Debug.Log("Slaves Aren't People: ForceRecountPatch: Adjusting MarketValue of slave: " + p.Name + " to: " + (p.MarketValue * 0.75f + marketValue));
                    }
                }
                Traverse.Create(__instance).Field("wealthPawns").SetValue(wealthPawns);
            }
            if (SlavesArentPeople.SAP_Settings.debugLog)
                Debug.Log("Slaves Aren't People: ForceRecountPatch: Adjusted wealthPawns: " + wealthPawns);
        }
    }
}
