using System.Collections.Generic;

namespace ArmorRepair
{
    public class ModSettings
    {
        public bool EnableAutoRepairPrompt { get; set; } = true;
        public bool AutoRepairMechsWithDestroyedComponents { get; set; } = true;
        public bool AutoRepairStructure { get; set; } = true;
        public bool ScaleStructureRepairTimeByTonnage { get; set; } = true;

        public List<RepairCostFactor> StructureRepairCostByTag { get; set; } = [];
        public List<RepairCostFactor> ArmorRepairCostByTag { get; set; } = [];
    }
}