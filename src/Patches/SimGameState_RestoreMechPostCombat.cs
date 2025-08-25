using BattleTech;
using UnityEngine;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Creates repair work orders for structure, components, and armor for each mech at the end of combat.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "RestoreMechPostCombat")]
    public static class SimGameState_RestoreMechPostCombat
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void Prefix(ref bool __runOriginal, SimGameState __instance, MechDef mech)
        {
            if (!__runOriginal) return;

            WorkOrderEntry_MechLab newWorkOrder = null;
            ProcessStructureRepairs(__instance, mech, ref newWorkOrder);
            ProcessComponentRepairs(__instance, mech, ref newWorkOrder);
            ProcessArmorRepairs(__instance, mech, ref newWorkOrder);

            // If any repair sub-orders were created, submit the main work order.
            if (newWorkOrder?.SubEntryCount > 0)
            {
                Helpers.SubmitTempWorkOrder(newWorkOrder);
            }

            // This logic is from the original method; it resets destroyed components to a functional state.
            foreach (MechComponentRef component in mech.Inventory)
            {
                if (component.DamageLevel == ComponentDamageLevel.NonFunctional)
                {
                    component.DamageLevel = ComponentDamageLevel.Functional;
                }
            }

            __runOriginal = false;
        }

        private static void ProcessStructureRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
        {
            if (!Main.Settings.EnableStructureRepair || !mech.NeedsStructureRepair())
                return;

            foreach (var location in Globals.repairPriorities.Values)
            {
                var locationLoadout = mech.GetLocationLoadoutDef(location);
                float currentStructure = locationLoadout.CurrentInternalStructure;
                float definedStructure = mech.GetChassisLocationDef(location).InternalStructure;

                if (currentStructure < definedStructure)
                {
                    workOrder ??= Helpers.CreateBaseMechLabOrder(sim, mech);

                    int structureDifference = (int)Mathf.Abs(currentStructure - definedStructure);
                    var repairWorkOrder = sim.CreateMechRepairWorkOrder(mech.GUID, location, structureDifference);
                    workOrder.AddSubEntry(repairWorkOrder);
                }
            }
        }

        private static void ProcessComponentRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
        {
            if (!mech.HasDamagedComponents())
                return;

            MechLabPanel_LoadMech.CurrentMech = mech;
            try
            {
                foreach (var component in mech.Inventory)
                {
                    if (component.DamageLevel == ComponentDamageLevel.Penalized)
                    {
                        workOrder ??= Helpers.CreateBaseMechLabOrder(sim, mech);

                        var repairWorkOrder = sim.CreateComponentRepairWorkOrder(component, true);
                        workOrder.AddSubEntry(repairWorkOrder);
                    }
                }
            }
            finally
            {
                // Clear the static field to avoid side effects.
                MechLabPanel_LoadMech.CurrentMech = null;
            }
        }

        private static void ProcessArmorRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
        {
            if (!mech.NeedArmorRepair())
                return;

            foreach (var location in Globals.repairPriorities.Values)
            {
                var locationLoadout = mech.GetLocationLoadoutDef(location);
                var chassisLocationDef = mech.GetChassisLocationDef(location);

                int armorDifference = (int)Mathf.Abs(locationLoadout.CurrentArmor - locationLoadout.AssignedArmor);
                if (chassisLocationDef.HasRearArmor())
                {
                    armorDifference += (int)Mathf.Abs(locationLoadout.CurrentRearArmor - locationLoadout.AssignedRearArmor);
                }

                if (armorDifference > 0)
                {
                    workOrder ??= Helpers.CreateBaseMechLabOrder(sim, mech);

                    var armorWorkOrder = sim.CreateMechArmorModifyWorkOrder(
                        mech.GUID,
                        location,
                        armorDifference,
                        (int)locationLoadout.AssignedArmor,
                        (int)locationLoadout.AssignedRearArmor
                    );

                    // Reset assigned armor to prevent free armor reset.
                    locationLoadout.AssignedArmor = Mathf.CeilToInt(locationLoadout.CurrentArmor);
                    locationLoadout.AssignedRearArmor = Mathf.CeilToInt(locationLoadout.CurrentRearArmor);

                    workOrder.AddSubEntry(armorWorkOrder);
                }
            }
        }
    }
}