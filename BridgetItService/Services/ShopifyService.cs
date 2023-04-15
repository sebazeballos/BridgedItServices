using BridgetItService.Contracts;
using Microsoft.ServiceFabric.Services.Communication;
using ShopifySharp;

namespace BridgetItService.Services
{
    public class ShopifyService : IShopifyService
    {
        public async Task<IEnumerable<Product>> PublishProducts(IEnumerable<Product> products)
        {
            var url = "bridged-it-services.myshopify.com";
            var accessToken = "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";
            if (products != null)
            {
                var service = new ProductService(url, accessToken);
                try
                {
                    foreach(Product product in products)
                    {
                        var response = await service.CreateAsync(product);
                    }
                }catch (Exception ex)
                {
                    throw new ServiceException($"REQUEST {HttpMethods.Post} to {url}", ex.ToString());
                }
                return products;
            }
            else
            {
                return products;
            }
        }
    }
}
