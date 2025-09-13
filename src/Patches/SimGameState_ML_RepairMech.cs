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
        public static void Prefix(ref bool __runOriginal, SimGameState __instance, WorkOrderEntry_RepairMechStructure order)
        {
            if (__runOriginal == false) return;
            if (order.IsMechLabComplete) return;

            MechDef mechByID = __instance.GetMechByID(order.MechLabParent.MechID);
            if (mechByID == null)
                return;

            LocationLoadoutDef locationLoadoutDef = mechByID.GetLocationLoadoutDef(order.Location);
            locationLoadoutDef.CurrentInternalStructure = mechByID.GetChassisLocationDef(order.Location).InternalStructure;
            mechByID.RefreshBattleValue();
            order.SetMechLabComplete(true);
            __runOriginal = false;
        }
    }
}