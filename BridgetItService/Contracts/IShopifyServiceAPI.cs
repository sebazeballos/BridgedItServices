using BridgetItService.Models;
using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IShopifyServiceAPI
    {
        public Task PublishProducts(InfinityPosProducts products);
        public Task PublishProduct(InfinityPOSProduct product);
        Task GetTransacctions(DateTime time);
    }
}
