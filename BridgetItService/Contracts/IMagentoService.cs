using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;

namespace BridgetItService.Contracts
{
    public interface IMagentoService
    {
        public Task PublishProducts(InfinityPosProducts products);
        public Task GetOrders(string startDate);
        public Task GetRefunds(string startDate);
        public Task<InfinityPosProducts> GetProductsInInfinity(InfinityPosProducts products);
        public Task GetOrder();
    }
}
