namespace ArmorRepair
{
    public struct RepairCost
    {
        public string Tag;
        public float ArmorTPCost;
        public float ArmorCBCost;
        public float StructureTPCost;
        public float StructureCBCost;
        public float InstallTPCost;
        public float InstallCBCost;
        public float RepairTPCost;
        public float RepairCBCost;
    }

    public class ModSettings
    {
        #region logging
        public bool Debug { get; set; } = false;
        #endregion logging

        #region game
        public bool EnableStructureRepair { get; set; } = true;
        public bool ScaleStructureCostByTonnage { get; set; } = true;
        public bool ScaleArmorCostByTonnage { get; set; } = true;
        public bool EnableAutoRepairPrompt { get; set; } = true;
        public bool AutoRepairMechsWithDestroyedComponents { get; set; } = true;

        public string ArmorCategory = "Armor";
        public string StructureCategory = "Structure";

        public RepairCost[] RepairCostByTag;

        public void Complete()
        {
            if (RepairCostByTag != null)
                for (int i = 0; i < RepairCostByTag.Length; i++)
                {
                    if (RepairCostByTag[i].ArmorCBCost <= 0f)
                        RepairCostByTag[i].ArmorCBCost = 1;
                    if (RepairCostByTag[i].ArmorTPCost <= 0f)
                        RepairCostByTag[i].ArmorTPCost = 1;

                    if (RepairCostByTag[i].StructureTPCost <= 0f)
                        RepairCostByTag[i].StructureTPCost = 1;
                    if (RepairCostByTag[i].StructureCBCost <= 0f)
                        RepairCostByTag[i].StructureCBCost = 1;

                    if (RepairCostByTag[i].RepairCBCost <= 0f)
                        RepairCostByTag[i].RepairCBCost = 1f;
                    if (RepairCostByTag[i].RepairTPCost <= 0f)
                        RepairCostByTag[i].RepairTPCost = 1f;

                    if (RepairCostByTag[i].InstallCBCost <= 0f)
                        RepairCostByTag[i].InstallCBCost = 1f;
                    if (RepairCostByTag[i].InstallTPCost <= 0f)
                        RepairCostByTag[i].InstallTPCost = 1f;
                }
        }
        #endregion game
    }
}
