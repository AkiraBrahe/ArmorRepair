using BattleTech;
using System;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(BattleTech.WorkOrderEntry_ModifyMechArmor))]
    [HarmonyPatch(
    [
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(int),
        typeof(ChassisLocations),
        typeof(int),
        typeof(int),
        typeof(int),
        typeof(string)
    ])]
    [HarmonyPatch(MethodType.Constructor)]
    public static class WorkOrderEntry_ModifyMechArmor
    {
        public static void Prefix(ref bool __runOriginal, ref int cbillCost, ref int techCost, int desiredFrontArmor, int desiredRearArmor)
        {
            try
            {
                if (__runOriginal == false) { return; }
                float techCostModifier = 0.01f; // Modify int based armor techCosts to a pseudo float
                float num = techCost * techCostModifier;
                techCost = Mathf.CeilToInt(num);
                cbillCost = Mathf.CeilToInt(cbillCost);

            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }
    }
}