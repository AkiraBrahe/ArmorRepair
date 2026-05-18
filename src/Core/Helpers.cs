using ArmorRepair.Patches;
using BattleTech;
using System;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Core
{
    public static class Helpers
    {
        #region Work Orders

        public static void ProcessStructureRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
        {
            if (!Main.Settings.AutoRepairStructure || !mech.NeedsStructureRepair())
                return;

            foreach (var location in repairPriorities.Values)
            {
                var locationLoadout = mech.GetLocationLoadoutDef(location);
                float currentStructure = locationLoadout.CurrentInternalStructure;
                float definedStructure = mech.GetChassisLocationDef(location).InternalStructure;

                if (currentStructure < definedStructure)
                {
                    workOrder ??= CreateBaseMechLabOrder(sim, mech);

                    int structureDifference = (int)Mathf.Abs(currentStructure - definedStructure);
                    var repairWorkOrder = sim.CreateMechRepairWorkOrder(mech.GUID, location, structureDifference);
                    workOrder.AddSubEntry(repairWorkOrder);
                }
            }
        }

        public static void ProcessArmorRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
        {
            if (!mech.NeedArmorRepair())
                return;

            foreach (var location in repairPriorities.Values)
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
                    workOrder ??= CreateBaseMechLabOrder(sim, mech);

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

        public static void ProcessComponentRepairs(SimGameState sim, MechDef mech, ref WorkOrderEntry_MechLab workOrder)
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
                        workOrder ??= CreateBaseMechLabOrder(sim, mech);

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

        /// <summary>
        /// Submits a mech lab work order to the temporary queue, which will be processed later by the player.
        /// </summary>
        public static void SubmitTempWorkOrder(WorkOrderEntry_MechLab workOrder)
        {
            try
            {
                tempMechLabQueue.Add(workOrder);
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

        #endregion

        #region Repair Prompt

        public static int FilterMechsWithDestroyedComponents(SimGameState sim)
        {
            int originalCount = tempMechLabQueue.Count;
            tempMechLabQueue.RemoveAll(order =>
            {
                var mech = sim.GetMechByID(order.MechID);
                return mech.HasDestroyedComponents();
            });
            return originalCount - tempMechLabQueue.Count;
        }

        public static void ShowRepairPrompt(SimGameState sim, int mechRepairCount, int skipMechCount)
        {
            var notificationQueue = sim.GetInterruptQueue();
            string skipMechCountDisplayed = GetMechCountDescription(skipMechCount, isForSkipped: true);

            // If all mechs were skipped, show a simple notification.
            if (mechRepairCount <= 0)
            {
                string message = $"Boss, {skipMechCountDisplayed} destroyed components. I'll leave the repairs for you to review.\n\n";
                notificationQueue.QueuePauseNotification(
                    "'Mech Repairs Needed",
                    message,
                    sim.GetCrewPortrait(SimGameCrew.Crew_Yang),
                    string.Empty,
                    tempMechLabQueue.Clear,
                    "OK"
                );
                return;
            }

            // Calculate total repair costs
            int cbills = tempMechLabQueue.Sum(o => o.GetCBillCost());
            int techCost = tempMechLabQueue.Sum(o => o.GetCost());

            // Calculate tech cost in days
            int techDays = 1;
            if (techCost > 0 && sim.MechTechSkill > 0)
            {
                techDays = Mathf.CeilToInt((float)techCost / sim.MechTechSkill);
            }

            string mechRepairCountDisplayed = GetMechCountDescription(mechRepairCount, isForSkipped: false);
            string finalMessage = BuildFinalPromptMessage(mechRepairCountDisplayed, cbills, techDays, skipMechCount, skipMechCountDisplayed);

            notificationQueue.QueuePauseNotification(
                "'Mech Repairs Needed",
                finalMessage,
                sim.GetCrewPortrait(SimGameCrew.Crew_Yang),
                string.Empty,
                () => ProcessRepairsAndClearQueue(sim),
                "Yes",
                tempMechLabQueue.Clear,
                "No"
            );
        }

        internal static string GetMechCountDescription(int count, bool isForSkipped)
        {
            return count <= 0
                ? string.Empty
                : isForSkipped
                ? count switch
                {
                    1 => "one of the 'Mechs is damaged but has",
                    2 => "two of the 'Mechs are damaged but have",
                    3 => "three of the 'Mechs are damaged but have",
                    4 => "a whole lance is damaged but has",
                    8 => "two lances are damaged but have",
                    12 => "all of our 'Mechs are damaged but have",
                    _ => $"{count} of the 'Mechs are damaged but have",
                }
                : count switch
                {
                    1 => "one of our 'Mechs was",
                    2 => "a couple of our 'Mechs were",
                    3 => "three of our 'Mechs were",
                    4 => "a whole lance was",
                    8 => "two lances were",
                    12 => "all of our 'Mechs were",
                    _ => $"{count} of our 'Mechs were",
                };
        }

        internal static string BuildFinalPromptMessage(string mechRepairCountDisplayed, int cbills, int techDays, int skipMechCount, string skipMechCountDisplayed)
        {
            string costString = $"It'll cost <color=#DE6729>{'¢'}{cbills:n0}</color> and {techDays} days for";
            string question = "Want my crew to get started?";

            if (skipMechCount > 0)
            {
                string skipMessagePart = $"{skipMechCountDisplayed} destroyed components, so I'll leave those repairs to you.";
                return $"Boss, {mechRepairCountDisplayed} damaged. {costString} these repairs. {question}\n\nAlso, {skipMessagePart}\n\n";
            }
            else
            {
                return $"Boss, {mechRepairCountDisplayed} damaged on the last engagement. {costString} the repairs. {question}";
            }
        }

        public static void ProcessRepairsAndClearQueue(SimGameState sim)
        {
            foreach (var workOrder in tempMechLabQueue)
            {
                SubmitWorkOrder(sim, workOrder);
            }
            tempMechLabQueue.Clear();
        }

        #endregion

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
                    var factor = Main.Settings.StructureRepairCostByTag.FirstOrDefault(r => r.ItemID == item.ComponentDefID);
                    if (factor != null) return factor;
                }
            }

            foreach (string tag in mech.Chassis.ChassisTags)
            {
                var factor = Main.Settings.StructureRepairCostByTag.FirstOrDefault(r => r.Tag == tag);
                if (factor != null) return factor;
            }

            return null;
        }

        #endregion

        #region Status Evaluation

        /// <summary>
        /// Evaluates whether a given mech needs any armor repaired.
        /// </summary>
        public static bool NeedArmorRepair(this MechDef mech)
        {
            foreach (var cLoc in repairPriorities.Values)
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
            foreach (var cLoc in repairPriorities.Values)
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

        #endregion

        #region Location Properties

        /// <summary>
        /// Evaluates whether a given chassis location has rear armor.
        /// </summary>
        public static bool HasRearArmor(this LocationDef chassisLocationDef) =>
            chassisLocationDef.Location is ChassisLocations.CenterTorso or
                                           ChassisLocations.LeftTorso or
                                           ChassisLocations.RightTorso;

        #endregion
    }
}