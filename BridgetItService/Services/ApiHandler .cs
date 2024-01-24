using BridgetItService.Contracts;
using BridgetItService.Data;
using BridgetItService.MapperFactory;
using BridgetItService.Models;
using BridgetItService.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShopifySharp;
using System;

namespace BridgetItService.Services
{
    public class ApiHandler
    {
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IShopifyServiceAPI _shopifyService;
        private readonly IMagentoService _magentoService;
        private readonly IOptions<EcommerceSettings> _options;
        private readonly ILogger<ApiHandler> _logger;
        private readonly IConfiguration _configuration;
        public ApiHandler(IInfinityPOSClient infinityPOSClient, IShopifyServiceAPI shopifyService, IServiceProvider serviceProvider, IMagentoService magentoService, ILogger<ApiHandler> logger, IConfiguration configuration)
        {
            _options = serviceProvider.GetService<IOptions<EcommerceSettings>>();
            _infinityPOSClient = infinityPOSClient;
            _shopifyService = shopifyService;
            _magentoService = magentoService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task UpdateShopifyAsync(DateTime time)
        {
            var checkTimeProducts = await GetLastProductUpdate();
            if (checkTimeProducts == null)
            {
                checkTimeProducts = time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            var  checkTimeTransactions = await GetLastTransactionUpdate() ?? time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            if (checkTimeTransactions == null)
            {
                checkTimeTransactions = time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            if (_options.Value.Type != "0")
            {
                var products = await _infinityPOSClient.GetProducts(checkTimeProducts);
                if (_options.Value.Type == "2")
                {
                    //await _shopifyService.PublishProducts(products);
                    //await _shopifyService.PublishProducts(await _infinityPOSClient.AddStock(checkTime));
                }
                if (_options.Value.Type == "1")
                {
                    await _magentoService.PublishProducts(await _infinityPOSClient.AddStock(products, checkTimeProducts));
                    await _magentoService.GetOrders(checkTimeTransactions);
                    await _magentoService.GetRefunds(checkTimeTransactions);
                }
            }
        }

        public async Task<string?> GetLastProductUpdate()
        {
            using (var context = new BridgedItContext(_configuration))
            {

                try
                {
                    var lastUpdate = await context.Product
                                          .OrderByDescending(p => p.LastUpdate)
                                          .Select(p => p.LastUpdate)
                                          .FirstOrDefaultAsync();

                    return lastUpdate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public async Task<string?> GetLastTransactionUpdate()
        {
            using (var context = new BridgedItContext(_configuration))
            {

                try
                {
                    var date = await context.Transaction
                             .OrderByDescending(t => t.SentTime)
                             .Select(t => t.SentTime)
                             .FirstOrDefaultAsync();
                    return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public async Task SyncronizePlatformsAsync(string time) => await SyncronizeTriquestra(time);

        public async Task SyncronizeTriquestra(string checkTime)
        {
            //var products = await _infinityPOSClient.SetProductAsFalse(checkTime);
            //await _infinityPOSClient.PostTransactionS();
            //await _magentoService.GetOrder();
            //var infinityProducts = await _infinityPOSClient.GetProducts(checkTime);
            //await _magentoService.GetRefunds(checkTime);
            await _magentoService.GetOrders(checkTime);
            //await _magentoService.PublishProducts(await _infinityPOSClient.AddStock(infinityProducts, checkTime));
        }
    }
}


