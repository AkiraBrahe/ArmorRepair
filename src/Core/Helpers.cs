using BattleTech;
using System;
using System.Linq;
using UnityEngine;

namespace ArmorRepair
{
    class Helpers
    {
        /// <summary>
        /// Submits a MechLab work order to the temporary queue, which will be processed later by the player.
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
        /// Submits a MechLab work order to the game's Mech Lab queue to actually be processed.
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
        /// Creates a base MechLab work order for a given MechDef.
        /// </summary>
        public static WorkOrderEntry_MechLab CreateBaseMechLabOrder(SimGameState __instance, MechDef mech)
        {
            try
            {
                string mechGUID = mech.GUID;
                string mechName = mech.Description?.Name != null ? mech.Description.Name : "Unknown";

                return new WorkOrderEntry_MechLab(
                    WorkOrderType.MechLabGeneric,
                    "MechLab-BaseWorkOrder",
                    string.Format("Modify 'Mech - {0}", mechName),
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

        /// <summary>
        /// Evaluates whether a given mech needs any armor repaired.
        /// </summary>
        public static bool CheckArmorDamage(MechDef mech)
        {
            bool mechNeedsRepair = false;

            for (int index = 0; index < Globals.repairPriorities.Count; index++)
            {
                ChassisLocations cLoc = Globals.repairPriorities.ElementAt(index).Value;
                LocationLoadoutDef loadout = mech.GetLocationLoadoutDef(cLoc);

                int armorDifference = loadout == mech.CenterTorso || loadout == mech.RightTorso || loadout == mech.LeftTorso
                    ? (int)Mathf.Abs(loadout.CurrentArmor - loadout.AssignedArmor) + (int)Mathf.Abs(loadout.CurrentRearArmor - loadout.AssignedRearArmor)
                    : (int)Mathf.Abs(loadout.CurrentArmor - loadout.AssignedArmor);

                if (armorDifference > 0)
                {
                    mechNeedsRepair = true;
                    break;
                }
            }

            return mechNeedsRepair;
        }

        /// <summary>
        /// Evaluates whether a given mech needs any structure repaired.
        /// </summary>
        public static bool CheckStructureDamage(MechDef mech)
        {
            bool mechNeedsRepair = false;

            for (int index = 0; index < Globals.repairPriorities.Count; index++)
            {

                ChassisLocations cLoc = Globals.repairPriorities.ElementAt(index).Value;
                LocationLoadoutDef loadout = mech.GetLocationLoadoutDef(cLoc);

                float currentStructure = loadout.CurrentInternalStructure;
                float maxStructure = mech.GetChassisLocationDef(cLoc).InternalStructure;
                int structureDifference = (int)Mathf.Abs(currentStructure - maxStructure);

                if (structureDifference > 0)
                {
                    mechNeedsRepair = true;
                    break;
                }
            }

            return mechNeedsRepair;
        }

        /// <summary>
        /// Evaluates whether a given mech has any destroyed components.
        /// </summary>
        public static bool CheckDestroyedComponents(MechDef mech)
        {
            bool destroyedComponents = false;

            foreach (MechComponentRef mechComponentRef in mech.Inventory)
            {
                if (mechComponentRef.DamageLevel == ComponentDamageLevel.Destroyed)
                {
                    destroyedComponents = true;
                    break;
                }
            }

            return destroyedComponents;
        }

        /// <summary>
        /// Evaluates whether a given mech has any damaged components
        /// </summary>
        public static bool CheckDamagedComponents(MechDef mech)
        {
            bool damagedComponents = false;

            foreach (MechComponentRef mechComponentRef in mech.Inventory)
            {
                if (mechComponentRef.DamageLevel == ComponentDamageLevel.Penalized)
                {
                    damagedComponents = true;
                    break;
                }
            }

            return damagedComponents;
        }
    }
}
