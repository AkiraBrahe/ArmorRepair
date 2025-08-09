using BattleTech;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
    public static class SimGameState_CreateComponentInstallWorkOrder
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void ChangeCost(MechComponentRef mechComponent, ChassisLocations newLocation, ref WorkOrderEntry_InstallComponent __result)
        {
            if (newLocation == ChassisLocations.None)
                return;
            if (MechLabPanel_LoadMech.CurrentMech == null || MechLabPanel_LoadMech.CurrentMech.Chassis == null)
                return;
            if (mechComponent.Def == null)
                return;

            if (__result == null)
                return;


            float tpmod = 1;
            float cbmod = 1;

            foreach (var tag in Main.Settings.RepairCostByTag)
            {
                if (MechLabPanel_LoadMech.CurrentMech.Chassis.ChassisTags.Contains(tag.Tag))
                {
                    tpmod *= tag.InstallTPCost;
                    cbmod *= tag.InstallCBCost;
                }

                if (mechComponent.Def.ComponentTags.Contains(tag.Tag))
                {
                    tpmod *= tag.InstallTPCost;
                    cbmod *= tag.InstallCBCost;
                }

            }

            if (tpmod != 1 || cbmod != 1)
            {
                var trav = new Traverse(__result);
                if (tpmod != 1)
                {
                    var cost = trav.Field<int>("Cost");
                    int new_cost = Mathf.CeilToInt(cost.Value * tpmod);
                    cost.Value = new_cost;
                }

                if (cbmod != 1)
                {
                    var cost = trav.Field<int>("CBillCost");
                    int new_cost = Mathf.CeilToInt(cost.Value * cbmod);
                    cost.Value = new_cost;
                }

            }
        }
    }
}