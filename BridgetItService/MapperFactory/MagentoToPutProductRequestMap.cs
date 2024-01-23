using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;

namespace BridgetItService.MapperFactory
{
    public class PostToPutProductRequestMap : IMap<MagentoProduct, PutMagentoProduct>
    {
        public PutMagentoProduct Map(MagentoProduct magentoProduct)
            => new PutMagentoProduct
            {
                Sku = magentoProduct.Sku,
                Price = magentoProduct.Price,
                Status = magentoProduct.Status,
                ExtensionAttributes = Ext(magentoProduct)
            };

        public ExtensionAttributes Ext(MagentoProduct product)
        {
            if(product.ExtensionAttributes != null)
            {
                return new ExtensionAttributes
                {
                    StockItem = new StockItem
                    {
                        IsInStock = true,
                        Qty = (long)product.ExtensionAttributes.StockItem.Qty
                    }
                };
            }
            else
            {
                return new ExtensionAttributes
                {
                    StockItem = new StockItem
                    {
                        IsInStock = true,
                        Qty = 0
                    }
                };
            }
        }
    }
}
