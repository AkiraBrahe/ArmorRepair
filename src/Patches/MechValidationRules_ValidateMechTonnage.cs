using BattleTech;
using Localize;
using System.Collections.Generic;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Flags up a warning in the mech lab when a mech has destroyed components.
    /// </summary>
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechTonnage")]
    public static class MechValidationRules_ValidateMechTonnage
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void Postfix(MechDef mechDef, ref Dictionary<MechValidationType, List<Text>> errorMessages)
        {
            for (int i = 0; i < mechDef.Inventory.Length; i++)
            {
                MechComponentRef component = mechDef.Inventory[i];
                if (component.DamageLevel == ComponentDamageLevel.Destroyed)
                {
                    errorMessages[MechValidationType.Underweight].Add(new Text("DESTROYED COMPONENT: 'Mech has destroyed components"));
                    break;
                }
            }
        }
    }
}