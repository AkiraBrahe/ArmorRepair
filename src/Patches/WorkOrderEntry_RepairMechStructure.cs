using BattleTech;
using HarmonyLib;
using System;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(BattleTech.WorkOrderEntry_RepairMechStructure), new Type[]
    {
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(int),
        typeof(ChassisLocations),
        typeof(int),
        typeof(int),
        typeof(string)
    })]
    [HarmonyPatch(MethodType.Constructor)]
    public static class WorkOrderEntry_RepairMechStructure
    {
        private static void Prefix(ref bool __runOriginal, ref int cbillCost, ref int techCost, int structureAmount)
        {
            try
            {
                if (__runOriginal == false) { return; }
                Logger.LogDebug("Structure WO Costing: ");
                Logger.LogDebug("*********************");
                Logger.LogDebug("techCost: " + techCost);
                Logger.LogDebug("cBillCost: " + cbillCost);
                Logger.LogDebug("*********************");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}