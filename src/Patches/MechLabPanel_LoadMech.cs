using BattleTech;
using BattleTech.UI;

namespace ArmorRepair.Patches
{
    /// <summary>
    /// Loads the current MechDef when a new mech is loaded in the mech lab.
    /// </summary>
    [HarmonyPatch(typeof(MechLabPanel), "LoadMech")]
    public static class MechLabPanel_LoadMech
    {
        public static MechDef CurrentMech = null;

        [HarmonyPrefix]
        public static void SetMech(ref bool __runOriginal, MechDef newMechDef)
        {
            if (__runOriginal == false) return;
            CurrentMech = newMechDef;
        }
    }
}