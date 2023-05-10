namespace BridgetItService.Models
{
    public class InfinityPOSProduct
    {
        public string ProductCode { get; set; } 
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Description { get; set; }
        public decimal StandardSellingPrice { get; set; }
        public string ProductType { get; set; }
        public string SupplierCode { get; set; }
        public long SellableQuantity { get; set; }
        public IList<CustomFields?> CustomFields { get; set; }
        public IList<AlternativeSellingPrices?> AlternativeSellingPrices { get; set; }
    }
}
