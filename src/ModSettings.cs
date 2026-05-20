using System.Collections.Generic;

namespace ArmorRepair
{
    public class ModSettings
    {
        public bool EnableAutoRepairPrompt { get; set; } = true;
        public bool AutoRepairMechsWithDestroyedComponents { get; set; } = true;
        public bool AutoRepairStructure { get; set; } = true;
        public bool ScaleStructureRepairTimeByTonnage { get; set; } = true;
        public float PrototypeEndoFerroRepairCostMultiplier { get; set; } = 3.0f;
        public float ClanTechRepairCostMultiplier { get; set; } = 1.5f;

        public List<RepairCostFactor> StructureRepairCostByTag { get; set; } = [];
        public List<RepairCostFactor> ArmorRepairCostByTag { get; set; } = [];
    }
}