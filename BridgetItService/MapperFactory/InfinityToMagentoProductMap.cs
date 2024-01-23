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
                if (product != null)
                {
                    if (product.CustomFields != null)
                    {
                        product.Created = new DateTime(product.Created.Year, product.Created.Month, product.Created.Day, 0, 0, 0);
                        product.Updated = new DateTime(product.Updated.Year, product.Updated.Month, product.Updated.Day, 0, 0, 0);
                        if ( product.CustomFields.Any(cf => cf.FieldName == "Web Enabled") && product.CustomFields.FirstOrDefault(cf => cf.FieldName == "Web Enabled").FieldValue == "True")
                        {
                            if (product.Updated != product.Created)
                            {
                                magentoProducts.Product.Add(new MagentoProduct
                                {
                                    Sku = product.ProductCode,
                                    Name = product.Description,
                                    TypeId = "simple",
                                    AttributeSetId = 4,
                                    Status = 1,
                                    Price = product.StandardSellingPrice,
                                    ExtensionAttributes = new ExtensionAttributes
                                    {
                                        StockItem = new StockItem
                                        {
                                            IsInStock = true,
                                            Qty = CheckQty((long)product.SellableQuantity)
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
                                        Visibility = 1,
                                        TypeId = "simple",
                                        AttributeSetId = 4,
                                        Status = 1,
                                        Price = product.StandardSellingPrice,
                                        ExtensionAttributes = new ExtensionAttributes
                                        {
                                            StockItem = new StockItem
                                            {
                                                IsInStock = true,
                                                Qty = CheckQty((long)product.SellableQuantity)
                                            }
                                        }
                                    });
                            }
                        }
                        else
                        {
                            if (product.CustomFields.Any(cf => cf.FieldName == "Web Enabled"))
                            {
                                magentoProducts.Product.Add(new MagentoProduct
                                {
                                    Sku = product.ProductCode,
                                    Name = product.Description,
                                    TypeId = "simple",
                                    AttributeSetId = 4,
                                    Visibility = 1,
                                    Status = 2,
                                    Price = product.StandardSellingPrice,
                                    ExtensionAttributes = new ExtensionAttributes
                                    {
                                        StockItem = new StockItem
                                        {
                                            IsInStock = true,
                                            Qty = CheckQty((long)product.SellableQuantity)
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            return magentoProducts;
        }

        public long CheckQty(long qty)
        {
            if(qty < 0)
            {
                qty = 0;
            }
            return qty;
        }
    }
}
