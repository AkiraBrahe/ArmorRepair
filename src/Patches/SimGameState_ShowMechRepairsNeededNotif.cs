using BattleTech;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "ShowMechRepairsNeededNotif")]
    public static class SimGameState_ShowMechRepairsNeededNotif
    {
        public static void Prefx(ref bool __runOriginal, SimGameState __instance)
        {
            if (__runOriginal == false) { return; }
            if (Main.Settings.EnableAutoRepairPrompt)
            {
                __instance.CompanyStats.Set("COMPANY_NotificationViewed_BattleMechRepairsNeeded", __instance.DaysPassed);
                __runOriginal = false; // Suppress original method
            }
            else
            {
                __runOriginal = true; // Do nothing if the player isn't using our Yang prompt functionality.
            }

        }
    }
}