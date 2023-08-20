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
            magentoProducts.DeletedProduct = new List<string>();
            foreach (InfinityPOSProduct product in infinityPosProducts.Products) {
                product.Created = new DateTime(product.Created.Year, product.Created.Month, product.Created.Day, product.Created.Hour, product.Created.Minute, 0);
                product.Updated = new DateTime(product.Updated.Year, product.Updated.Month, product.Updated.Day, product.Updated.Hour, product.Updated.Minute, 0);
                if (product.CustomFields != null && product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Web Enabled").FieldValue == "True")
                {
                    if (product.Updated != product.Created)
                    {
                        magentoProducts.Product.Add(new MagentoProduct
                        {
                            Sku = product.ProductCode,
                            Name = product.Description,
                            TypeId = "simple",
                            Visibility = 4,
                            Price = product.StandardSellingPrice,
                            AttributeSetId = 4,
                            ExtensionAttributes = new ExtensionAttributes
                            {
                                StockItem = new StockItem
                                {
                                    IsInStock = true,
                                    Qty = CheckQty(product.SellableQuantity)
                                }
                            }
                        });
                    }
                    else
                    {
                        magentoProducts.Product.Add(new MagentoProduct
                        {
                            Sku = product.ProductCode,
                            Name = product.Description,
                            TypeId = "simple",
                            Visibility = 1,
                            Price = product.StandardSellingPrice,
                            AttributeSetId = 4,
                            ExtensionAttributes = new ExtensionAttributes
                            {
                                StockItem = new StockItem
                                {
                                    IsInStock = true,
                                    Qty = CheckQty(product.SellableQuantity)
                                }
                            }
                        });
                    }
                }
                else
                {
                    if (product.Updated != product.Created)
                    {
                        magentoProducts.DeletedProduct.Add(product.ProductCode);
                    }
                }                
            }
            return magentoProducts;
        }

        public string CheckQty(long qty)
        {
            if(qty < 0)
            {
                qty = 0;
            }
            return qty.ToString();
        }
    }
}
