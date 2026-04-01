namespace ArmorRepair
{
    public class ModSettings
    {
        #region logging

        public bool Debug { get; set; } = false;

        #endregion logging

        #region game

        public bool EnableStructureRepair { get; set; } = true;
        public bool EnableAutoRepairPrompt { get; set; } = true;
        public bool AutoRepairMechsWithDestroyedComponents { get; set; } = true;

        public string ArmorPrefix { get; set; } = "Gear_Armor_";
        public string StructurePrefix { get; set; } = string.Empty;

        public System.Collections.Generic.List<RepairCostFactor> RepairCostByTag { get; set; } = [];

        #endregion game
    }
}