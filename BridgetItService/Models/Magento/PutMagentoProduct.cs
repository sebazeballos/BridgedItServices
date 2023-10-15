namespace BridgetItService.Models.Magento
{
    public class PutMagentoProduct
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public int Visibility { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public int AttributeSetId { get; set; }
        public ExtensionAttributes? ExtensionAttributes { get; set; }
    }
}
