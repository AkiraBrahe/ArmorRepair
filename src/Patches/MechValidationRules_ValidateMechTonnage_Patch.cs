using System;
using System.Collections.Generic;
using BattleTech;
using HarmonyLib;
using Localize;

namespace ArmorRepair
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechTonnage")]
    public static class MechValidationRules_ValidateMechTonnage_Patch
    {
        public static void Postfix(MechDef mechDef, ref Dictionary<MechValidationType, List<Text>> errorMessages)
        {
            try
            {
                for (int i = 0; i < mechDef.Inventory.Length; i++)
                {
                    MechComponentRef mechComponentRef = mechDef.Inventory[i];
                    if (mechComponentRef.DamageLevel == ComponentDamageLevel.Destroyed)
                    {
                        Logger.LogDebug("Flagging destroyed component warning: " + mechDef.Name);
                        errorMessages[MechValidationType.Underweight].Add(new Text("DESTROYED COMPONENT: 'Mech has destroyed components"));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}