using BattleTech;
using CustomComponents;
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
        [HarmonyWrapSafe]
        public static void Postfix(SimGameState __instance, string mechSimGameUID, int armorDiff, ref BattleTech.WorkOrderEntry_ModifyMechArmor __result)
        {
            // If no armor was changed, or if the work order wasn't created, do nothing.
            if (armorDiff == 0 || __result == null)
                return;

            var mech = __instance.ActiveMechs.Values.FirstOrDefault(md => md.GUID == mechSimGameUID);
            if (mech == null)
                return;

            (float tpmod, float cbmod) = CalculateArmorRepairModifiers(mech);

            // Apply tonnage modifier if enabled.
            float mechTonnageModifier = 1f;
            if (Main.Settings.ScaleArmorCostByTonnage)
            {
                mechTonnageModifier = mech.Chassis.Tonnage * 0.01f;
            }

            int techCost = Mathf.CeilToInt(__result.Cost * tpmod * mechTonnageModifier);
            int cbillCost = Mathf.CeilToInt(__result.CBillCost * cbmod * mechTonnageModifier);

            __result.Cost = techCost;
            __result.CBillCost = cbillCost;
        }

        public static (float tpmod, float cbmod) CalculateArmorRepairModifiers(MechDef mech)
        {
            var armorItem = mech.Inventory.FirstOrDefault(item => item.IsCategory(Main.Settings.ArmorCategory));
            var armor = armorItem?.GetComponent<ArmorRepairFactor>();

            float tpmod = armor?.ArmorTPCost ?? 1f;
            float cbmod = armor?.ArmorCBCost ?? 1f;

            (float tagTpMod, float tagCbMod) = Helpers.CalculateModifiers(mech, armorItem, f => f.ArmorTPCost, f => f.ArmorCBCost);

            return (tpmod * tagTpMod, cbmod * tagCbMod);
        }
    }
}