using BridgetItService.Models;

namespace BridgetItService.Contracts
{
    public interface IMagentoService
    {
        public Task PublishProducts(InfinityPosProducts products);
    }
}
