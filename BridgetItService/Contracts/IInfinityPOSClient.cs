using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IInfinityPOSClient
    {
        Task<string> GetAuthentication();
        Task<string> GetProducts(string startDate);
    }
}
