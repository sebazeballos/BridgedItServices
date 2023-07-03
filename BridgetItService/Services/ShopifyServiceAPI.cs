using System;
using System.Fabric.Query;
using System.Text.Json;
using System.Text;
using BridgetItService.Contracts;
using ShopifySharp;
using ShopifySharp.Filters;
using Microsoft.Extensions.Options;
using BridgetItService.Settings;
using BridgetItService.Models.Inifnity;

namespace BridgetItService.Services
{
    public class ShopifyServiceAPI : IShopifyServiceAPI
    {

        private readonly IMap<InfinityPOSProduct, Product> _infinityToShopifyMap;
        private readonly IInfinityPOSClient _infinityPOSClient;
        private readonly IOptions<ShopifySettings> _options;
        public ShopifyServiceAPI(IInfinityPOSClient infinityPOSClient, IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetService<IOptions<ShopifySettings>>();
            _infinityPOSClient = infinityPOSClient;
            _infinityToShopifyMap = serviceProvider.GetService<IMap<InfinityPOSProduct, Product>>();
        }
        public async Task PublishProducts(InfinityPosProducts products)
        {
            if (products != null)
            {
                var service = new ProductService(_options.Value.url, _options.Value.accessToken);

                foreach (InfinityPOSProduct product in products.Products)
                {
                    Product shopifyProduct = _infinityToShopifyMap.Map(product);
                    if (product.CustomFields != null && product.CustomFields.Any(cf => cf.FieldName != "Ecomm Code"))
                    {
                        try
                        {
                            Product response = await service.CreateAsync(shopifyProduct);
                            product.CustomFields = new List<CustomFields>();

                            product.CustomFields.Add(new CustomFields
                            {
                                FieldName = "Ecomm Code",
                                FieldValue = response.Id.ToString()
                            });
                            await _infinityPOSClient.PutProductInInfinity(product); 
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            await service.UpdateAsync(long.Parse(product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Ecomm Code").FieldValue), shopifyProduct);
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }
               
            }
        }

        public async Task PublishProduct(InfinityPOSProduct? product)
        {
            if (product != null)
            {
                var service = new ProductService(_options.Value.url, _options.Value.accessToken);
                Product shopifyProduct = _infinityToShopifyMap.Map(product);

                try
                {
                    await service.UpdateAsync(long.Parse(product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Ecomm Code").FieldValue), shopifyProduct);
                }
                catch (Exception ex)
                {

                }

            }              
        }
        public async Task GetTransacctions(DateTime time)
        {
            var service = new TenderTransactionService(_options.Value.url, _options.Value.accessToken);
            try
            {
                var transactions = await service.ListAsync(new TenderTransactionListFilter
                {
                    ProcessedAtMax = time
                });
                var p = transactions;
                var jsonString = JsonSerializer.Serialize(p.Items.FirstOrDefault());
            }
            catch (ShopifyException ex)
            {
                var msj = ex.InnerException;
            }
            
        }
    }
}
