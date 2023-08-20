using BridgetItService.Models.Inifnity;
using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IInfinityPOSClient
    {
        Task<string> GetAuthentication();
        Task<InfinityPosProducts> GetProducts(string startDate);
        Task PutProductInInfinity(PutProductInInfinity product);
        Task<InfinityPosProducts> AddStock(InfinityPosProducts products, string startDate);
        Task PostTransaction(Invoice invoice);
        Task PutProductListInInfinity(PutProducts products);
        Task<PutProducts> SetProductAsFalse(string startDate);  
    }
}
