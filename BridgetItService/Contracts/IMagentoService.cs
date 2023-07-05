using BridgetItService.Models.Inifnity;

namespace BridgetItService.Contracts
{
    public interface IMagentoService
    {
        public Task PublishProducts(InfinityPosProducts products);
        public Task GetOrders();
        public Task GetRefunds();
    }
}
