using System;
using System.Fabric.Query;
using BridgetItService.Contracts;
using BridgetItService.Models;
using ShopifySharp;
using ShopifySharp.Filters;

namespace BridgetItService.Services
{
    public class ShopifyServiceAPI : IShopifyServiceAPI
    {

        private readonly IMap<InfinityPOSProduct, Product> _infinityToShopifyMap;
        private readonly IInfinityPOSClient _infinityPOSClient;
        public ShopifyServiceAPI(IInfinityPOSClient infinityPOSClient, IServiceProvider serviceProvider)
        {
            _infinityPOSClient = infinityPOSClient;
            _infinityToShopifyMap = serviceProvider.GetService<IMap<InfinityPOSProduct, Product>>();
        }
        public async Task PublishProducts(IList<InfinityPOSProduct> products)
        {
            var url = "bridged-it-services.myshopify.com";
            var accessToken = "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";
            if (products != null)
            {
                var service = new ProductService(url, accessToken);

                foreach (InfinityPOSProduct product in products)
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
                            
                            //product.CustomFields.Add(new CustomFields
                            //{
                            //    FieldName = "Web_Item",
                            //    FieldValue = product.Updated.ToString()
                            //});
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

                            //product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Web_Item").FieldValue = product.Updated.ToString();

                            //await _infinityPOSClient.PutProductInInfinity(product);
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }
               
            }
        }

        public async Task PublishProduct(InfinityPOSProduct product)
        {
            var url = "bridged-it-services.myshopify.com";
            var accessToken = "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";
            if (product != null)
            {
                var service = new ProductService(url, accessToken);
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
            var url = "bridged-it-services.myshopify.com";
            var accessToken = "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";
            var service = new TenderTransactionService(url, accessToken);
            try
            {
                var transactions = await service.ListAsync(new TenderTransactionListFilter
                {
                    ProcessedAtMax = time
                });
                var p = transactions;
            }
            catch (Exception ex)
            {
                var msj = ex.InnerException;
            }
            
        }
    }
}
