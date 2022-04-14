using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SlavesArentPeople
{
    public static class CaravanWealthForStoryTellerPatch
    {
        public static bool Prefix(Caravan __instance, ref float __result)
        {
            if (!__instance.IsPlayerControlled)
            {
                __result = 0.0f;
                return false;
            }
            float num = 0.0f;
            for (int index = 0; index < __instance.pawns.Count; ++index)
            {
                //adjust item wealth
                if (SlavesArentPeople.SAP_Settings.hasPFC)
                    num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(__instance.pawns[index]) * PrepareForCombat.PrepareForCombat.Instance.percentWealthItems;
                else if (SlavesArentPeople.SAP_Settings.slavesAreFurniture && __instance.pawns[index].IsSlave)
                    num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(__instance.pawns[index]) * 0.5f;
                else if (SlavesArentPeople.SAP_Settings.slavesArePrisoners && __instance.pawns[index].IsSlave)
                    num += 0f;
                else
                    num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(__instance.pawns[index]);

                if (__instance.pawns[index].Faction == Faction.OfPlayer)
                {
                    //adjust market value of slaves
                    float marketValue = __instance.pawns[index].MarketValue;
                    if (__instance.pawns[index].IsSlave)
                        if (SlavesArentPeople.SAP_Settings.hasPFC)
                            marketValue *= (float)SlavesArentPeople.SAP_Settings.wealthSlaves / 100f - 0.75f;
                        else if (SlavesArentPeople.SAP_Settings.slavesAreFurniture)
                            marketValue *= 0.5f; // decrease to 50%
                        else if (SlavesArentPeople.SAP_Settings.slavesArePrisoners)
                            marketValue *= 0f; //decrease to 0%
                        else
                            marketValue *= 1f; //100&
                    num += marketValue;
                }
            }
            __result = num * 0.7f;
            return false;
        }
    }
}
