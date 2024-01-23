namespace BridgetItService.Models.Magento
{
    public class PutMagentoProduct
    {
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public ExtensionAttributes? ExtensionAttributes { get; set; }
    }
}
