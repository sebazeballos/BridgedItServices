namespace BridgetItService.Models.Inifnity
{
    public class SellingRules
    {
        public int SellingPromptCode { get; set; }
        public string? TransactionCaptureTemplateCode { get; set; }
        public string? SellingStartDate { get; set; }
        public string? SellingEndDate { get; set; }
        public bool? SingleQuantityPerSaleLine { get; set; }
        public bool? AllowSaleWithOtherProducts { get; set; }

    }
}
