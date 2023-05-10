using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IShopifyService
    {
        public Task<IEnumerable<Product>> PublishProducts(IEnumerable<Product> products);
    }
}
