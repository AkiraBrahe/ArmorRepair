using BattleTech;
using CustomComponents;
using System;
using UnityEngine;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(SimGameState), "CreateMechRepairWorkOrder")]
    public static class SimGameState_CreateMechRepairWorkOrder
    {
        public static void Postfix(ref SimGameState __instance, ref string mechSimGameUID, ref ChassisLocations location, ref int structureCount, ref BattleTech.WorkOrderEntry_RepairMechStructure __result)
        {
            try
            {

                float mechTonnageModifier = 1f;
                // Original method code, this is still needed to work out zero structure modifiers 
                string id = string.Format("MechLab - RepairMech - {0}", __instance.GenerateSimGameUID());
                bool is_repaired = false;
                float cbmod = 1f;
                float tpmod = 1f;

                foreach (MechDef mechDef in __instance.ActiveMechs.Values)
                {
                    if (mechDef.GUID == mechSimGameUID)
                    {
                        if (mechDef.GetChassisLocationDef(location).InternalStructure == (float)structureCount)
                        {
                            is_repaired = true;
                            break;
                        }

                        // If ScaleStructureCostByTonnage is enabled, make the mech tonnage work as a percentage tech cost reduction (95 tons = 0.95 or "95%" of the cost, 50 tons = 0.05 or "50%" of the cost etc)
                        if (Main.Settings.ScaleStructureCostByTonnage)
                        {
                            mechTonnageModifier = mechDef.Chassis.Tonnage * 0.01f;
                        }

                        StructureRepairFactor str = null;
                        MechComponentRef structitem = null;
                        foreach (var item in mechDef.Inventory)
                        {
                            if (item.IsCategory(Main.Settings.StructureCategory))
                            {
                                str = item.GetComponent<StructureRepairFactor>();
                                structitem = item;
                                break;
                            }
                        }

                        tpmod *= str?.StructureTPCost ?? 1;
                        cbmod *= str?.StructureCBCost ?? 1;


                        if (Main.Settings.RepairCostByTag != null && Main.Settings.RepairCostByTag.Length > 0)
                            foreach (var cost in Main.Settings.RepairCostByTag)
                            {
                                if (mechDef.Chassis.ChassisTags.Contains(cost.Tag))
                                {
                                    tpmod *= cost.StructureTPCost;
                                    cbmod *= cost.StructureCBCost;
                                }

                                if (structitem != null && structitem.Def.ComponentTags.Contains(cost.Tag))
                                {
                                    tpmod *= cost.StructureTPCost;
                                    cbmod *= cost.StructureCBCost;
                                }

                            }
                        break;
                    }
                }
                if (is_repaired)
                {
                    cbmod = __instance.Constants.MechLab.ZeroStructureCBillModifier;
                    tpmod = __instance.Constants.MechLab.ZeroStructureTechPointModifier;
                }



                int techCost = Mathf.CeilToInt(__instance.Constants.MechLab.StructureRepairTechPoints * structureCount * tpmod * mechTonnageModifier);
                int cbillCost = Mathf.CeilToInt(__instance.Constants.MechLab.StructureRepairCost * structureCount * cbmod * mechTonnageModifier);

                __result = new BattleTech.WorkOrderEntry_RepairMechStructure(id, string.Format("Repair 'Mech - {0}", location.ToString()), mechSimGameUID, techCost, location, structureCount, cbillCost, string.Empty);

            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }
    }
}