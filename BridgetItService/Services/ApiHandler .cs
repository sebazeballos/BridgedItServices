using BridgetItService.Contracts;
using BridgetItService.MapperFactory;
using BridgetItService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BridgetItService.Services
{
    public class ApiHandler
    {
        private readonly HttpClient _httpClient;
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IShopifyServiceAPI _shopifyService;

        public ApiHandler(HttpClient httpClient, IInfinityPOSClient infinityPOSClient, IShopifyServiceAPI shopifyService)
        {
            _httpClient = httpClient;
            _infinityPOSClient = infinityPOSClient;
            _shopifyService = shopifyService;
        }

        public async Task UpdateShopifyAsync(DateTime time)
        {
            var product = await _infinityPOSClient.GetProducts(time.ToString());
            if(product != null) { 
                await _shopifyService.PublishProducts(product.Products);
            }
            await _shopifyService.PublishProducts(await _infinityPOSClient.AddStock(time.ToString())); 
        }
    }
}
