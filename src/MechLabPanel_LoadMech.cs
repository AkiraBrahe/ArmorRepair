using BattleTech;
using BattleTech.UI;
using HarmonyLib;

namespace ArmorRepair
{
    [HarmonyPatch(typeof(MechLabPanel), "LoadMech")]
    public static class MechLabPanel_LoadMech
    {
        public static MechDef CurrentMech = null;

        [HarmonyPrefix]
        public static void SetMech(ref bool __runOriginal,MechDef newMechDef)
        {
            if (__runOriginal == false) { return; }
            CurrentMech = newMechDef;
        }
    }
}