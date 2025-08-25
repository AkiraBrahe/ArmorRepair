using BattleTech;
using UnityEngine;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Modifies armor repair costs to a pseudo-float value.
    /// </summary>
    [HarmonyPatch(typeof(BattleTech.WorkOrderEntry_ModifyMechArmor))]
    [HarmonyPatch([typeof(string), typeof(string), typeof(string), typeof(int), typeof(ChassisLocations), typeof(int), typeof(int), typeof(int), typeof(string)])]
    [HarmonyPatch(MethodType.Constructor)]
    public static class WorkOrderEntry_ModifyMechArmor
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void Prefix(ref bool __runOriginal, ref int cbillCost, ref int techCost)
        {
            if (__runOriginal == false) return;
            techCost = Mathf.CeilToInt(techCost * 0.01f);
            cbillCost = Mathf.CeilToInt(cbillCost);
        }
    }
}