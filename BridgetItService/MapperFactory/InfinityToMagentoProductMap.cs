using BridgetItService.Contracts;
using BridgetItService.Models;
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
                    Price = 100000,
                    AttributeSetId = 4
                });
            }
            return magentoProducts;
        }
    }
}
