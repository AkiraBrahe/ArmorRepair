using BattleTech;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Applies the repair cost modifiers for repairing components in the mech lab.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CreateComponentRepairWorkOrder")]
    public static class SimGameState_CreateComponentRepairWorkOrder
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(MechComponentRef mechComponent, WorkOrderEntry_RepairComponent __result)
        {
            var mech = MechLabPanel_LoadMech.CurrentMech;
            if (mech?.Chassis == null || mechComponent?.Def == null)
                return;

            (float tpmod, float cbmod) = CalculateRepairModifiers(mech, mechComponent);
            Helpers.ApplyModifiers(ref __result.Cost, ref __result.CBillCost, tpmod, cbmod);
        }

        public static (float tpmod, float cbmod) CalculateRepairModifiers(MechDef mech, MechComponentRef mechComponent) =>
            Helpers.CalculateModifiers(mech, mechComponent, f => f.RepairTPCost, f => f.RepairCBCost);
    }
}