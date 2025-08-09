using BattleTech;
using CustomComponents;
using System;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "CreateMechArmorModifyWorkOrder")]
    public static class SimGameState_CreateMechArmorModifyWorkOrder
    {
        public static void Postfix(ref SimGameState __instance,
            ref string mechSimGameUID,
            ref ChassisLocations location,
            ref int armorDiff, ref int frontArmor, ref int rearArmor, ref BattleTech.WorkOrderEntry_ModifyMechArmor __result)
        {
            string id = string.Format("MechLab - ModifyArmor - {0}", __instance.GenerateSimGameUID());

            try
            {
                float mechTonnageModifier = 1f;
                int techCost = 0;
                int cbillCost = 0;


                foreach (MechDef mechDef in __instance.ActiveMechs.Values)
                {
                    if (mechDef.GUID == mechSimGameUID)
                    {
                        ArmorRepairFactor armor = null;
                        MechComponentRef armoritem = null;
                        foreach (var item in mechDef.Inventory)
                        {
                            if (item.IsCategory(Main.Settings.ArmorCategory))
                            {
                                armor = item.GetComponent<ArmorRepairFactor>();
                                armoritem = item;
                                break;
                            }
                        }

                        float atpcost = armor?.ArmorTPCost ?? 1;
                        float acbcost = armor?.ArmorCBCost ?? 1;


                        if (Main.Settings.RepairCostByTag != null && Main.Settings.RepairCostByTag.Length > 0)
                            foreach (var cost in Main.Settings.RepairCostByTag)
                            {
                                if (mechDef.Chassis.ChassisTags.Contains(cost.Tag))
                                {
                                    atpcost *= cost.ArmorTPCost;
                                    acbcost *= cost.ArmorCBCost;
                                }

                                if (armoritem != null && armoritem.Def.ComponentTags.Contains(cost.Tag))
                                {
                                    atpcost *= cost.ArmorTPCost;
                                    acbcost *= cost.ArmorCBCost;
                                }

                            }


                        // If ScaleArmorCostByTonnage is enabled, make the mech tonnage work as a percentage tech cost reduction (95 tons = 0.95 or "95%" of the cost, 50 tons = 0.05 or "50%" of the cost etc)
                        if (Main.Settings.ScaleArmorCostByTonnage)
                        {
                            mechTonnageModifier = mechDef.Chassis.Tonnage * 0.01f;
                        }

                        float locationTechCost = armorDiff * mechTonnageModifier * __instance.Constants.MechLab.ArmorInstallTechPoints * atpcost;
                        float locationCbillCost = armorDiff * mechTonnageModifier * __instance.Constants.MechLab.ArmorInstallCost * acbcost;
                        techCost = Mathf.CeilToInt(locationTechCost);
                        cbillCost = Mathf.CeilToInt(locationCbillCost);
                    }
                }

                __result = new BattleTech.WorkOrderEntry_ModifyMechArmor(id, string.Format("Modify Armor - {0}", location.ToString()), mechSimGameUID, techCost, location, frontArmor, rearArmor, cbillCost, string.Empty);
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }
    }
}