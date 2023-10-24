namespace BridgetItService.Models.Inifnity
{
    public class Line
    {
        public int LineNumber { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public float Quantity { get; set; }
        public double UnitSellingPrice { get; set; }
        public double StandardUnitSellingPrice { get; set; }
        public double ExtendedSalesTax { get; set; }
    }
}