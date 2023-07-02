namespace BridgetItService.Models
{
    public class MagentoProduct
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public int Visibility { get; set; }
        public decimal Price { get; set; }
        public int AttributeSetId { get; set; }
        public ExtensionAttributes? ExtensionAttributes { get; set; }
    }
}
