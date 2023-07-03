using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using ShopifySharp;

namespace BridgetItService.MapperFactory
{
    public class InfinityToShopifyProductMap : IMap<InfinityPOSProduct, Product>
    {
        
        public Product Map(InfinityPOSProduct infinityPOSProduct)
        {
            return new Product
            {
                Title = infinityPOSProduct.Description,
                Vendor = infinityPOSProduct.SupplierCode,
                ProductType = infinityPOSProduct.ProductType,
                CreatedAt = infinityPOSProduct.Created,
                UpdatedAt = infinityPOSProduct.Updated,
                Variants = new List<ProductVariant> { new ProductVariant { Price = infinityPOSProduct.StandardSellingPrice,
                                                                           CompareAtPrice = GetPromotionalPrice(infinityPOSProduct),
                                                                           InventoryQuantity = infinityPOSProduct.SellableQuantity
                                                                           } },
                
                
            };
            
        }
        private decimal? GetPromotionalPrice(InfinityPOSProduct product)
        {
            if (product.AlternativeSellingPrices != null)
            {
                return product.AlternativeSellingPrices.FirstOrDefault(p => p.PriceListId == 8).PriceListSellingPrice;
            }
            else
            {
                return null;
            }
        }           

    }
}
