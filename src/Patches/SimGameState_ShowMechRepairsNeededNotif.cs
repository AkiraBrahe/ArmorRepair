using BattleTech;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Suppresses the default mech repair notification if the auto-repair prompt is enabled.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "ShowMechRepairsNeededNotif")]
    public static class SimGameState_ShowMechRepairsNeededNotif
    {
        [HarmonyPrefix]
        public static void Prefx(ref bool __runOriginal, SimGameState __instance)
        {
            if (__runOriginal == false) return;
            if (Main.Settings.EnableAutoRepairPrompt)
            {
                __instance.CompanyStats.Set("COMPANY_NotificationViewed_BattleMechRepairsNeeded", __instance.DaysPassed);
                __runOriginal = false;
            }
            else
            {
                __runOriginal = true;
            }
        }
    }
}