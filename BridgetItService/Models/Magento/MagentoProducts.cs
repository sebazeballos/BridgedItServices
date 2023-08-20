namespace BridgetItService.Models.Magento
{
    public class MagentoProducts
    {
        public IList<MagentoProduct>? Product { get; set; }
        public IList<string>? DeletedProduct { get; set; }
    }
}
