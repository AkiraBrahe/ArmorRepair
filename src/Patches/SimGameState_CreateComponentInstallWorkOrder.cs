using BattleTech;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Applies the repair cost modifiers for installing components in the mech lab.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
    public static class SimGameState_CreateComponentInstallWorkOrder
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(MechComponentRef mechComponent, ChassisLocations newLocation, ref WorkOrderEntry_InstallComponent __result)
        {
            if (mechComponent?.Def == null || newLocation == ChassisLocations.None || __result == null)
                return;

            var mech = MechLabPanel_LoadMech.CurrentMech;
            if (mech?.Chassis == null)
                return;

            (float tpmod, float cbmod) = CalculateInstallModifiers(mech, mechComponent);
            Helpers.ApplyModifiers(ref __result.Cost, ref __result.CBillCost, tpmod, cbmod);
        }

        public static (float tpmod, float cbmod) CalculateInstallModifiers(MechDef mech, MechComponentRef mechComponent) =>
            Helpers.CalculateModifiers(mech, mechComponent, f => f.InstallTPCost, f => f.InstallCBCost);
    }
}