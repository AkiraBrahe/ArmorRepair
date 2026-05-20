namespace ArmorRepair.Core
{
    public class RepairCostFactor
    {
        public string Tag { get; set; } = string.Empty;
        public float WeightMultiplier { get; set; } = 1f;
        public float PPTMultiplier { get; set; } = 1f;
        public float TPCost { get; set; } = 1f;
        public float CBCost { get; set; } = 1f;
    }
}