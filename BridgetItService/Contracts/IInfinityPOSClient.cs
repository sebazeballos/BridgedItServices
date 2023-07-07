using BridgetItService.Models.Inifnity;
using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IInfinityPOSClient
    {
        Task<string> GetAuthentication();
        Task<InfinityPosProducts> GetProducts(string startDate);
        Task PutProductInInfinity(InfinityPOSProduct product);
        Task<InfinityPosProducts> AddStock(InfinityPosProducts products, string startDate);
        Task PostTransaction(Invoice invoice);
    }
}
