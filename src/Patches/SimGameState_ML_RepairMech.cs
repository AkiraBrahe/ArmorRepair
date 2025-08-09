using BattleTech;
using System;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "ML_RepairMech")]
    public static class SimGameState_ML_RepairMech
    {
        public static void Prefix(ref bool __runOriginal, SimGameState __instance, BattleTech.WorkOrderEntry_RepairMechStructure order)
        {
            if (__runOriginal == false) { return; }
            if (order.IsMechLabComplete)
            {
                return;
            }
            else
            {
                try
                {
                    MechDef mechByID = __instance.GetMechByID(order.MechLabParent.MechID);
                    if (mechByID == null)
                    {
                        return;
                    }
                    LocationLoadoutDef locationLoadoutDef = mechByID.GetLocationLoadoutDef(order.Location);
                    locationLoadoutDef.CurrentInternalStructure = mechByID.GetChassisLocationDef(order.Location).InternalStructure;
                    // Original method resets currentArmor to assignedArmor here for some reason! Removed them from this override
                    mechByID.RefreshBattleValue();
                    order.SetMechLabComplete(true);
                    __runOriginal = false; // Prevent original method from firing
                }
                catch (Exception e)
                {
                    SimGameState.logger.LogException(e);
                }
            }
        }
    }
}