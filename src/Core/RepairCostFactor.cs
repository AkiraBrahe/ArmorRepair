namespace ArmorRepair.Core
{
    public class RepairCostFactor
    {
        public string Tag { get; set; }
        public float ArmorTPCost { get; set; }
        public float ArmorCBCost { get; set; }
        public float StructureTPCost { get; set; }
        public float StructureCBCost { get; set; }
        public float RepairTPCost { get; set; }
        public float RepairCBCost { get; set; }
        public float InstallTPCost { get; set; }
        public float InstallCBCost { get; set; }
    }
}