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
                Status = 2,
                Price = magentoProduct.Price,
                AttributeSetId = 4,
                ExtensionAttributes = Ext(magentoProduct)
            };

        public ExtensionAttributes? Ext(MagentoProduct product)
        {
            if(product.ExtensionAttributes.StockItem.Qty != 0)
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
                return null;
            }
        }
    }
}
