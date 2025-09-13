using BattleTech;
using System.Linq;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "ResolveCompleteContract")]
    public static class SimGameState_ResolveCompleteContract
    {
        /// <summary>
        /// Ensures the temporary queue is cleared before processing a new contract completion.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static void Prefix(ref bool __runOriginal, SimGameState __instance)
        {
            if (__runOriginal == false) return;
            Globals.tempMechLabQueue.Clear();
        }

        /// <summary>
        /// Prompts the player to approve or deny the queued mech repairs after completing a contract.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void Postfix(SimGameState __instance)
        {
            int skipMechCount = 0;
            if (Globals.tempMechLabQueue.Count <= 0) return;

            if (!Main.Settings.AutoRepairMechsWithDestroyedComponents)
            {
                skipMechCount = FilterMechsWithDestroyedComponents(__instance);
            }

            int mechRepairCount = Globals.tempMechLabQueue.Count;

            // No mechs to repair or report on.
            if (mechRepairCount <= 0 && skipMechCount <= 0)
            {
                Globals.tempMechLabQueue.Clear();
                return;
            }

            if (Main.Settings.EnableAutoRepairPrompt)
            {
                ShowRepairPrompt(__instance, mechRepairCount, skipMechCount);
            }
            else
            {
                ProcessRepairsAndClearQueue(__instance);
            }
        }

        private static int FilterMechsWithDestroyedComponents(SimGameState sim)
        {
            int originalCount = Globals.tempMechLabQueue.Count;
            Globals.tempMechLabQueue.RemoveAll(order =>
            {
                MechDef mech = sim.GetMechByID(order.MechID);
                return mech.HasDestroyedComponents();
            });
            return originalCount - Globals.tempMechLabQueue.Count;
        }

        private static void ShowRepairPrompt(SimGameState sim, int mechRepairCount, int skipMechCount)
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
                    Globals.tempMechLabQueue.Clear,
                    "OK"
                );
                return;
            }

            // Calculate summary of total repair costs from the temp work order queue
            int cbills = Globals.tempMechLabQueue.Sum(o => o.GetCBillCost());
            int techCost = Globals.tempMechLabQueue.Sum(o => o.GetCost());

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
                Globals.tempMechLabQueue.Clear,
                "No"
            );
        }

        private static string GetMechCountDescription(int count, bool isForSkipped)
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

        private static string BuildFinalPromptMessage(string mechRepairCountDisplayed, int cbills, int techDays, int skipMechCount, string skipMechCountDisplayed)
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

        private static void ProcessRepairsAndClearQueue(SimGameState sim)
        {
            foreach (WorkOrderEntry_MechLab workOrder in Globals.tempMechLabQueue)
            {
                Helpers.SubmitWorkOrder(sim, workOrder);
            }
            Globals.tempMechLabQueue.Clear();
        }
    }
}