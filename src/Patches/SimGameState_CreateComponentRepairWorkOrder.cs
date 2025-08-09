using BattleTech;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "CreateComponentRepairWorkOrder")]
    public static class SimGameState_CreateComponentRepairWorkOrder
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(MechComponentRef mechComponent, WorkOrderEntry_RepairComponent __result)
        {
            var mech = MechLabPanel_LoadMech.CurrentMech;
            if (mechComponent == null)
                return;

            float tpmod = 1;
            float cbmod = 1;

            foreach (var tag in Main.Settings.RepairCostByTag)
            {
                if (mech != null && mech.Chassis.ChassisTags.Contains(tag.Tag))
                {
                    tpmod *= tag.RepairTPCost;
                    cbmod *= tag.RepairCBCost;
                }

                if (mechComponent.Def.ComponentTags.Contains(tag.Tag))
                {
                    tpmod *= tag.RepairTPCost;
                    cbmod *= tag.RepairCBCost;
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