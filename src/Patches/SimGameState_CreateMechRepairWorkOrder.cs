using BattleTech;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Applies the repair cost modifiers for repairing structure in the mech lab.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CreateMechRepairWorkOrder")]
    public static class SimGameState_CreateMechRepairWorkOrder
    {
        [HarmonyPostfix]
        public static void Postfix(SimGameState __instance, string mechSimGameUID, ref WorkOrderEntry_RepairMechStructure __result)
        {
            var mech = __instance.ActiveMechs.Values.FirstOrDefault(md => md.GUID == mechSimGameUID);
            if (mech == null || __result == null)
                return;

            var factor = mech.GetRepairFactor(Main.Settings.StructurePrefix);
            float tpmod = factor?.TPCost ?? 1f;
            float cbmod = factor?.CBCost ?? 1f;

            float mechTonnageModifier = 1f;
            if (Main.Settings.ScaleStructureCostByTonnage)
            {
                mechTonnageModifier = mech.Chassis.Tonnage * 0.01f;
            }

            __result.Cost = Mathf.CeilToInt(__result.Cost * tpmod * mechTonnageModifier);
            __result.CBillCost = Mathf.CeilToInt(__result.CBillCost * cbmod * mechTonnageModifier);
        }
    }
}