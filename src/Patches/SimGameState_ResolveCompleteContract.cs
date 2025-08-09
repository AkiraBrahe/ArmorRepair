using BattleTech;
using BattleTech.UI;
using System;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "ResolveCompleteContract")]
    public static class SimGameState_ResolveCompleteContract
    {

        // Just for safety, ensure the temp queue in this mod is completely clear before we run any processing
        public static void Prefix(ref bool __runOriginal, SimGameState __instance)
        {
            if (__runOriginal == false) { return; }
            try
            {
                Globals.tempMechLabQueue.Clear();
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }

        // Run after completion of contracts and queue up any orders in the temp queue into the game's Mech Lab queue 
        public static void Postfix(SimGameState __instance)
        {
            try
            {
                // If there are any work orders in the temporary queue, prompt the player
                if (Globals.tempMechLabQueue.Count > 0)
                {
                    int cbills = 0;
                    int techCost = 0;
                    int mechRepairCount = 0;
                    int skipMechCount = 0;
                    string mechRepairCountDisplayed = String.Empty;
                    string skipMechCountDisplayed = String.Empty;
                    string skipMechMessage = String.Empty;
                    string finalMessage = String.Empty;

                    // If player has disabled auto repairing mechs with destroyed components, check for them and remove them from the temp queue before continuing
                    if (!Main.Settings.AutoRepairMechsWithDestroyedComponents)
                    {
                        for (int index = 0; index < Globals.tempMechLabQueue.Count; index++)
                        {
                            WorkOrderEntry_MechLab order = Globals.tempMechLabQueue[index];

                            bool destroyedComponents = false;
                            MechDef mech = __instance.GetMechByID(order.MechID);
                            destroyedComponents = Helpers.CheckDestroyedComponents(mech);

                            if (destroyedComponents)
                            {
                                // Remove this work order from the temp mech lab queue if the mech has destroyed components and move to next iteration
                                Globals.tempMechLabQueue.Remove(order);
                                destroyedComponents = false;
                                skipMechCount++;
                                index++;

                            }
                        }
                    }


                    // Calculate summary of total repair costs from the temp work order queue
                    for (int index = 0; index < Globals.tempMechLabQueue.Count; index++)
                    {
                        WorkOrderEntry_MechLab order = Globals.tempMechLabQueue[index];
                        MechDef mech = __instance.GetMechByID(order.MechID);
                        cbills += order.GetCBillCost();
                        techCost += order.GetCost();
                        mechRepairCount++;
                    }

                    mechRepairCount = Mathf.Clamp(mechRepairCount, 0, 4);

                    // If Yang's Auto Repair prompt is enabled, build a message prompt dialog for the player
                    if (Main.Settings.EnableAutoRepairPrompt)
                    {

                        // Calculate a friendly techCost of the work order in days, based on number of current mechtechs in the player's game.
                        if (techCost != 0 && __instance.MechTechSkill != 0)
                        {
                            techCost = Mathf.CeilToInt(techCost / __instance.MechTechSkill);
                        }
                        else
                        {
                            techCost = 1; // Safety in case of weird div/0
                        }

                        // Generate a quick friendly description of how many mechs were damaged in battle
                        switch (mechRepairCount)
                        {
                            case 1: { mechRepairCountDisplayed = "one of our 'Mechs was"; break; }
                            case 2: { mechRepairCountDisplayed = "a couple of the 'Mechs were"; break; }
                            case 3: { mechRepairCountDisplayed = "three of our 'Mechs were"; break; }
                            case 4: { mechRepairCountDisplayed = "our whole lance was"; break; }
                        }
                        // Generate a friendly description of how many mechs were damaged but had components destroyed
                        switch (skipMechCount)
                        {
                            case 1: { skipMechCountDisplayed = "one of the 'Mechs is damaged but has"; break; }
                            case 2: { skipMechCountDisplayed = "two of the 'Mechs are damaged but have"; break; }
                            case 3: { skipMechCountDisplayed = "three of the 'Mechs are damaged but have "; break; }
                            case 4: { skipMechCountDisplayed = "the whole lance is damaged but has"; break; }
                        }

                        // Check if there are any mechs to process
                        if (mechRepairCount > 0 || skipMechCount > 0)
                        {
                            // Setup the notification for mechs with damaged components that we might want to skip
                            skipMechMessage = skipMechCount > 0 && mechRepairCount == 0
                                ? $"{skipMechCountDisplayed} destroyed components. I'll leave the repairs for you to review."
                                : $"{skipMechCountDisplayed} destroyed components, so I'll leave those repairs to you.";

                            SimGameInterruptManager notificationQueue = __instance.GetInterruptQueue();

                            // If all of the mechs needing repairs have damaged components and should be skipped from auto-repair, change the message notification structure to make more sense (e.g. just have an OK button)
                            if (skipMechCount > 0 && mechRepairCount == 0)
                            {
                                finalMessage = $"Boss, {skipMechMessage} \n\n";

                                // Queue Notification
                                notificationQueue.QueuePauseNotification(
                                    "'Mech Repairs Needed",
                                    finalMessage,
                                    __instance.GetCrewPortrait(SimGameCrew.Crew_Yang),
                                    string.Empty,
                                    delegate
                                    {
                                    },
                                    "OK"
                                );
                            }
                            else
                            {
                                finalMessage = skipMechCount > 0
                                    ? $"Boss, {mechRepairCountDisplayed} damaged. " +
                                        $"It'll cost <color=#DE6729>{'¢'}{cbills.ToString():n0}</color> and {techCost} days for these repairs. " +
                                        $"Want my crew to get started?\n\n" +
                                        $"Also, {skipMechMessage}\n\n"
                                    : $"Boss, {mechRepairCountDisplayed} damaged on the last engagement. " +
                                        $"It'll cost <color=#DE6729>{'¢'}{cbills.ToString():n0}</color> and {techCost} days for the repairs. " +
                                        $"Want my crew to get started?";

                                notificationQueue.QueuePauseNotification(
                                    "'Mech Repairs Needed",
                                    finalMessage,
                                    __instance.GetCrewPortrait(SimGameCrew.Crew_Yang),
                                    string.Empty,
                                    delegate
                                    {
                                        foreach (WorkOrderEntry_MechLab workOrder in Globals.tempMechLabQueue.ToList())
                                        {
                                            Helpers.SubmitWorkOrder(__instance, workOrder);
                                            Globals.tempMechLabQueue.Remove(workOrder);
                                        }
                                    },
                                    "Yes",
                                    delegate
                                    {
                                        foreach (WorkOrderEntry_MechLab workOrder in Globals.tempMechLabQueue.ToList())
                                        {
                                            Globals.tempMechLabQueue.Remove(workOrder);
                                        }
                                    },
                                    "No"
                                );
                            }
                        }
                    }
                    else // If Auto Repair prompt is not enabled, just proceed with queuing the remaining temp queue work orders and don't notify the player
                    {
                        foreach (WorkOrderEntry_MechLab workOrder in Globals.tempMechLabQueue.ToList())
                        {
                            Helpers.SubmitWorkOrder(__instance, workOrder);
                            Globals.tempMechLabQueue.Remove(workOrder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.tempMechLabQueue.Clear();
                Main.Log.LogException(ex);
            }
        }
    }
}