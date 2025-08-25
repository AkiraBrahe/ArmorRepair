using BattleTech;
using CustomComponents;
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
        [HarmonyWrapSafe]
        public static void Postfix(SimGameState __instance, string mechSimGameUID, ChassisLocations location, int structureCount, ref WorkOrderEntry_RepairMechStructure __result)
        {
            var mech = __instance.ActiveMechs.Values.FirstOrDefault(md => md.GUID == mechSimGameUID);
            if (mech == null)
                return;

            float tpmod;
            float cbmod;

            // If location is at full structure, the game applies special modifiers.
            if (mech.GetChassisLocationDef(location).InternalStructure == structureCount)
            {
                tpmod = __instance.Constants.MechLab.ZeroStructureTechPointModifier;
                cbmod = __instance.Constants.MechLab.ZeroStructureCBillModifier;
            }
            else
            {
                (tpmod, cbmod) = CalculateStructureRepairModifiers(mech);
            }

            // Apply tonnage modifier if enabled.
            float mechTonnageModifier = 1f;
            if (Main.Settings.ScaleStructureCostByTonnage)
            {
                mechTonnageModifier = mech.Chassis.Tonnage * 0.01f;
            }

            int techCost = Mathf.CeilToInt(__instance.Constants.MechLab.StructureRepairTechPoints * structureCount * tpmod * mechTonnageModifier);
            int cbillCost = Mathf.CeilToInt(__instance.Constants.MechLab.StructureRepairCost * structureCount * cbmod * mechTonnageModifier);

            __result.Cost = techCost;
            __result.CBillCost = cbillCost;
        }

        public static (float tpmod, float cbmod) CalculateStructureRepairModifiers(MechDef mech)
        {
            var structItem = mech.Inventory.FirstOrDefault(item => item.IsCategory(Main.Settings.StructureCategory));
            var str = structItem?.GetComponent<StructureRepairFactor>();

            float tpmod = str?.StructureTPCost ?? 1f;
            float cbmod = str?.StructureCBCost ?? 1f;

            (float tagTpMod, float tagCbMod) = Helpers.CalculateModifiers(mech, structItem, f => f.StructureTPCost, f => f.StructureCBCost);

            return (tpmod * tagTpMod, cbmod * tagCbMod);
        }
    }
}