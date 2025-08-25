using BattleTech;
using System;
using UnityEngine;

namespace ArmorRepair
{
    public static class Helpers
    {
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

        /// <summary>
        /// Calculates the total cost modifiers for a given mech and its components based on tags.
        /// </summary>
        public static (float tpmod, float cbmod) CalculateModifiers(MechDef mech, MechComponentRef mechComponent, Func<dynamic, float> getTpMod, Func<dynamic, float> getCbMod)
        {
            float tpmod = 1f;
            float cbmod = 1f;

            if (Main.Settings.RepairCostByTag == null)
            {
                return (1f, 1f);
            }

            foreach (dynamic factor in Main.Settings.RepairCostByTag)
            {
                if (mech.Chassis.ChassisTags.Contains(factor.Tag))
                {
                    tpmod *= getTpMod(factor);
                    cbmod *= getCbMod(factor);
                }

                if (mechComponent != null && mechComponent.Def.ComponentTags.Contains(factor.Tag))
                {
                    tpmod *= getTpMod(factor);
                    cbmod *= getCbMod(factor);
                }
            }
            return (tpmod, cbmod);
        }

        /// <summary>
        /// Applies the given modifiers to the provided costs.
        /// </summary>
        public static void ApplyModifiers(ref int techCost, ref int cbillCost, float tpmod, float cbmod)
        {
            if (tpmod != 1f)
            {
                techCost = Mathf.CeilToInt(techCost * tpmod);
            }

            if (cbmod != 1f)
            {
                cbillCost = Mathf.CeilToInt(cbillCost * cbmod);
            }
        }

        /// <summary>
        /// Evaluates whether a given mech needs any armor repaired.
        /// </summary>
        public static bool NeedArmorRepair(this MechDef mech)
        {
            foreach (var cLoc in Globals.repairPriorities.Values)
            {
                LocationLoadoutDef loadout = mech.GetLocationLoadoutDef(cLoc);

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
                LocationLoadoutDef loadout = mech.GetLocationLoadoutDef(cLoc);

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
            foreach (MechComponentRef component in mech.Inventory)
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
            foreach (MechComponentRef component in mech.Inventory)
            {
                if (component.DamageLevel == ComponentDamageLevel.Penalized)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates whether a given chassis location has rear armor.
        /// </summary>
        public static bool HasRearArmor(this LocationDef chassisLocationDef)
        {
            return chassisLocationDef.Location == ChassisLocations.CenterTorso ||
                   chassisLocationDef.Location == ChassisLocations.LeftTorso ||
                   chassisLocationDef.Location == ChassisLocations.RightTorso;
        }
    }
}
