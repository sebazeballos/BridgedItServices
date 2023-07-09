using BridgetItService.Contracts;
using BridgetItService.MapperFactory;
using BridgetItService.Models;
using BridgetItService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace BridgetItService.Services
{
    public class ApiHandler
    {
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IShopifyServiceAPI _shopifyService;
        private readonly IMagentoService _magentoService;
        private readonly IOptions<EcommerceSettings> _options;

        public ApiHandler(IInfinityPOSClient infinityPOSClient, IShopifyServiceAPI shopifyService, IServiceProvider serviceProvider, IMagentoService magentoService)
        {
            _options = serviceProvider.GetService<IOptions<EcommerceSettings>>();
            _infinityPOSClient = infinityPOSClient;
            _shopifyService = shopifyService;
            _magentoService = magentoService;
        }

        public async Task UpdateShopifyAsync(DateTime time)
        {
            var checkTime = time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var products = await _infinityPOSClient.GetProducts(checkTime);
            if (_options.Value.Type == "2")
            {
                await _shopifyService.PublishProducts(products);
                //await _shopifyService.PublishProducts(await _infinityPOSClient.AddStock(checkTime));
            }
            else
            {
                await _magentoService.PublishProducts(products);
                //await _magentoService.PublishProducts(await _infinityPOSClient.AddStock(checkTime));
                await _magentoService.GetOrders(checkTime);
                await _magentoService.GetRefunds(checkTime);
            }
        }

        public async Task SyncronizePlatformsAsync(DateTime time)
        {
            var checkTime = time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var products = await _infinityPOSClient.GetProducts(checkTime);
            await _magentoService.PublishProducts(await _infinityPOSClient.AddStock(products, checkTime));
        }
    }
}
