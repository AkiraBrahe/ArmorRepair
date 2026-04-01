using BattleTech;
using System;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Core
{
    public static class Helpers
    {
        #region Work Orders

        /// <summary>
        /// Submits a mech lab work order to the temporary queue, which will be processed later by the player.
        /// </summary>
        public static void SubmitTempWorkOrder(WorkOrderEntry_MechLab workOrder)
        {
            try
            {
                Globals.tempMechLabQueue.Add(workOrder);
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Submits a mech lab work order to the game's mech lab queue to actually be processed.
        /// </summary>
        public static void SubmitWorkOrder(SimGameState simGame, WorkOrderEntry_MechLab workOrder)
        {
            try
            {
                simGame.MechLabQueue.Insert(0, workOrder);
                simGame.InitializeMechLabEntry(workOrder, workOrder.GetCBillCost());
                simGame.UpdateMechLabWorkQueue(false);
                simGame.AddFunds(-workOrder.GetCBillCost(), "ArmorRepair", true);
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Creates a base mech lab work order for a given MechDef.
        /// </summary>
        public static WorkOrderEntry_MechLab CreateBaseMechLabOrder(SimGameState __instance, MechDef mech)
        {
            try
            {
                string mechGUID = mech.GUID;
                string mechName = mech.Description?.Name != null ? mech.Description.Name : "Unknown";

                return new WorkOrderEntry_MechLab(WorkOrderType.MechLabGeneric,
                    "MechLab-BaseWorkOrder",
                    $"Modify 'Mech - {mechName}",
                    mechGUID,
                    0,
                    string.Format(__instance.Constants.Story.GeneralMechWorkOrderCompletedText, mechName));
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
                return null;
            }
        }

        #endregion Work Orders

        #region Cost Modifiers

        /// <summary>
        /// Gets the repair cost factor for a given mech or component.
        /// </summary>
        public static RepairCostFactor GetRepairFactor(this MechDef mech, string itemPrefix)
        {
            if (!string.IsNullOrEmpty(itemPrefix))
            {
                var item = mech.Inventory.FirstOrDefault(i => i.ComponentDefID.StartsWith(itemPrefix, StringComparison.Ordinal));
                if (item != null)
                {
                    var factor = Main.Settings.RepairCostByTag.FirstOrDefault(r => r.ItemID == item.ComponentDefID);
                    if (factor != null) return factor;
                }
            }

            foreach (string tag in mech.Chassis.ChassisTags)
            {
                var factor = Main.Settings.RepairCostByTag.FirstOrDefault(r => r.Tag == tag);
                if (factor != null) return factor;
            }

            return null;
        }

        #endregion Cost Modifiers

        #region Status Evaluation

        /// <summary>
        /// Evaluates whether a given mech needs any armor repaired.
        /// </summary>
        public static bool NeedArmorRepair(this MechDef mech)
        {
            foreach (var cLoc in Globals.repairPriorities.Values)
            {
                var loadout = mech.GetLocationLoadoutDef(cLoc);

                int armorDifference = loadout == mech.CenterTorso || loadout == mech.RightTorso || loadout == mech.LeftTorso
                    ? (int)Mathf.Abs(loadout.CurrentArmor - loadout.AssignedArmor) + (int)Mathf.Abs(loadout.CurrentRearArmor - loadout.AssignedRearArmor)
                    : (int)Mathf.Abs(loadout.CurrentArmor - loadout.AssignedArmor);

                if (armorDifference > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates whether a given mech needs any structure repaired.
        /// </summary>
        public static bool NeedsStructureRepair(this MechDef mech)
        {
            foreach (var cLoc in Globals.repairPriorities.Values)
            {
                var loadout = mech.GetLocationLoadoutDef(cLoc);

                float currentStructure = loadout.CurrentInternalStructure;
                float maxStructure = mech.GetChassisLocationDef(cLoc).InternalStructure;

                if ((int)Mathf.Abs(currentStructure - maxStructure) > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates whether a given mech has any destroyed components.
        /// </summary>
        public static bool HasDestroyedComponents(this MechDef mech)
        {
            foreach (var component in mech.Inventory)
            {
                if (component.DamageLevel == ComponentDamageLevel.Destroyed)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates whether a given mech has any damaged components
        /// </summary>
        public static bool HasDamagedComponents(this MechDef mech)
        {
            foreach (var component in mech.Inventory)
            {
                if (component.DamageLevel == ComponentDamageLevel.Penalized)
                    return true;
            }
            return false;
        }

        #endregion Status Evaluation

        #region Location Properties

        /// <summary>
        /// Evaluates whether a given chassis location has rear armor.
        /// </summary>
        public static bool HasRearArmor(this LocationDef chassisLocationDef) =>
            chassisLocationDef.Location is ChassisLocations.CenterTorso or
                                           ChassisLocations.LeftTorso or
                                           ChassisLocations.RightTorso;

        #endregion Location Properties
    }
}