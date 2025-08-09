using BattleTech;
using Localize;
using System;
using System.Collections.Generic;

namespace ArmorRepair.Patches
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechTonnage")]
    public static class MechValidationRules_ValidateMechTonnage
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
                        errorMessages[MechValidationType.Underweight].Add(new Text("DESTROYED COMPONENT: 'Mech has destroyed components"));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }
    }
}