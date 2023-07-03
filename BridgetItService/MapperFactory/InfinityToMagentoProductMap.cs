using BridgetItService.Contracts;
using BridgetItService.Models;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;
using ShopifySharp;

namespace BridgetItService.MapperFactory
{
    public class InfinityToMagentoProductMap : IMap<InfinityPosProducts, MagentoProducts>
    {

        public MagentoProducts Map(InfinityPosProducts infinityPosProducts)
        {
            var magentoProducts = new MagentoProducts();
            magentoProducts.Product = new List<MagentoProduct>();
            foreach (InfinityPOSProduct product in infinityPosProducts.Products) {

                magentoProducts.Product.Add(new MagentoProduct
                {
                    Sku = product.ProductCode,
                    Name = product.Description,
                    TypeId = "simple",
                    Visibility = 4,
                    Price = product.StandardSellingPrice,
                    AttributeSetId = 4,
                    ExtensionAttributes = new ExtensionAttributes { 
                                                        StockItem = new StockItem
                                                        {
                                                            IsInStock = true,
                                                            Qty = product.SellableQuantity.ToString() 
                                                        }
                                                    }
                });
            }
            return magentoProducts;
        }
    }
}
