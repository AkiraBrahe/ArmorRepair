using BattleTech;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Prevents structure repair work orders from resetting armor.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "ML_RepairMech")]
    public static class SimGameState_ML_RepairMech
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void Prefix(ref bool __runOriginal, SimGameState __instance, WorkOrderEntry_RepairMechStructure workOrder)
        {
            if (__runOriginal == false) return;
            if (workOrder.IsMechLabComplete) return;

            MechDef mechByID = __instance.GetMechByID(workOrder.MechLabParent.MechID);
            if (mechByID == null)
                return;

            LocationLoadoutDef locationLoadoutDef = mechByID.GetLocationLoadoutDef(workOrder.Location);
            locationLoadoutDef.CurrentInternalStructure = mechByID.GetChassisLocationDef(workOrder.Location).InternalStructure;
            mechByID.RefreshBattleValue();
            workOrder.SetMechLabComplete(true);
            __runOriginal = false;
        }
    }
}