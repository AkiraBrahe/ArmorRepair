using BattleTech;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Applies the repair cost modifiers for repairing armor in the mech lab.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CreateMechArmorModifyWorkOrder")]
    public static class SimGameState_CreateMechArmorModifyWorkOrder
    {
        [HarmonyPostfix]
        public static void Postfix(SimGameState __instance, string mechSimGameUID, int armorDiff, ref BattleTech.WorkOrderEntry_ModifyMechArmor __result)
        {
            if (armorDiff == 0 || __result == null)
                return;

            var mech = __instance.ActiveMechs.Values.FirstOrDefault(md => md.GUID == mechSimGameUID);
            if (mech == null)
                return;

            var factor = mech.GetRepairFactor(Main.Settings.ArmorPrefix);
            float tpmod = factor?.TPCost ?? 1f;
            float cbmod = factor?.CBCost ?? 1f;

            float mechTonnageModifier = 1f;
            if (Main.Settings.ScaleArmorCostByTonnage)
            {
                mechTonnageModifier = mech.Chassis.Tonnage * 0.01f;
            }

            __result.Cost = Mathf.CeilToInt(__result.Cost * tpmod * mechTonnageModifier);
            __result.CBillCost = Mathf.CeilToInt(__result.CBillCost * cbmod * mechTonnageModifier);
        }
    }
}