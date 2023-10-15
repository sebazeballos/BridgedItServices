namespace BridgetItService.Models.Inifnity
{
    public class PutProductInInfinity
    {
        public string ProductCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Description { get; set; }
        public decimal StandardSellingPrice { get; set; }
        public string ProductType { get; set; }
        public IList<CustomFields> CustomFields { get; set; }
        public string SupplierCode { get; set; }
        public decimal? LatestCostPrice { get; set; }
        public int? HierarchyPosition { get; set; }
        public decimal? Margin { get; set; }
        public decimal? MarginPercentage { get; set; }
        public decimal? MarkupPercentage { get; set;}
        public int? SellingPromptCode { get; set; }
        public string? Medias { get; set; }
        public string? Sku { get; set; }
        public string? Unit { get; set; }
        public double? TargetMarginPercentage { get; set; }
        public bool? NonStock { get; set; }
        public bool? Serialised { get; set; }
        public bool? Commission { get; set; }
        public List<AlternativeSellingPrices>? AlternativeSellingPrices { get; set; }
        public string? HasMedias { get; set; }
        public bool? SiteSpecific { get; set; }
        public int? TaxCode { get; set; }
        public SellingRules? SellingRules { get; set; }
        public string? ProductDiscountGroup { get; set; }
        public bool Archive { get; set; }
        public string? ProductCodeParentUnit { get; set;}
        public int? ConversionToParentUnit { get; set; }
        public string? LeadTime { get; set; }
    }
}
