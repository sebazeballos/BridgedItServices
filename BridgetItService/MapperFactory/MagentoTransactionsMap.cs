using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;
using ShopifySharp;

namespace BridgetItService.MapperFactory
{
    public class MagentoTransactionsMap : IMap<MagentoOrder, Invoices>
    {
        public readonly string DELIVERY_SKU = "987878465789987";
        public readonly string WALLPAPER_SKU = "7777777571301";
        public readonly string FABRIC_SKU_ = "7777777571318";
        public Invoices Map(MagentoOrder order)
        {
            var invoices = new Invoices();
            invoices.Invoice = new List<Invoice>();
            foreach (Item item in order.Items)
            {
                invoices.Invoice.Add(new Invoice
                {
                    SalesPersonCode = "1",
                    SiteCode = "1",
                    Lines = MapLines(item),
                    Payments = MapPayment(item),
                });
            }
            return invoices;
        }
        private List<Line> MapLines(Item item)
        {
            var lines = new List<Line>();
            var lineNumber = 1;
                foreach(TransactionItem transactionItem in item.Items)
                {
                    if (transactionItem.BasePriceInclTax > 0)
                    {
                        if (transactionItem.Name.Contains("Wallpaper")) {
                            transactionItem.Sku = WALLPAPER_SKU;
                        }
                        if (transactionItem.Name.Contains("Fabric"))
                        {
                        transactionItem.Sku = FABRIC_SKU_;
                        }
                        if (transactionItem.Name.Contains("International Air Freight"))
                        {
                            transactionItem.Sku = DELIVERY_SKU;
                        }
                    var discount = 0.0;
                        if (transactionItem.BaseDiscountAmount != 0 && transactionItem.QtyOrdered != 0)
                        {
                            discount = transactionItem.BaseDiscountAmount / transactionItem.QtyOrdered;
                            transactionItem.BasePriceInclTax = transactionItem.BasePriceInclTax - discount;
                        }
                        lines.Add(new Line
                        {
                            LineNumber = lineNumber++,
                            ProductCode = CheckProductCode(transactionItem.Sku),
                            Quantity = transactionItem.QtyOrdered,
                            UnitSellingPrice = transactionItem.BasePriceInclTax,
                            StandardUnitSellingPrice = transactionItem.BasePriceInclTax,
                            ExtendedSalesTax = item.TaxAmount
                        });
                    }
                }
                if (item.Payment.ShippingAmount > 0)
                {
                    lines.Add(new Line
                    {
                        LineNumber = lineNumber++,
                        ProductCode = DELIVERY_SKU,
                        Quantity = 1,
                        UnitSellingPrice = item.Payment.ShippingAmount,
                        StandardUnitSellingPrice = item.Payment.ShippingAmount,
                        ExtendedSalesTax = 0
                    });
                }

            return lines;
        }
        private List<Payment> MapPayment(Item item)
        {
            var payments = new List<Payment>();
            var paymentLineNumber = 1;
            if (item.Payment.Method.ToUpper() == "PAYMENTEXPRESS_PXPAY2")
                item.Payment.Method = "PAYMENTEXPRESS";
            payments.Add(new Payment
            {
                PaymentLineNumber = paymentLineNumber++,
                TenderType = item.Payment.Method.ToUpper(),
                TenderDescription = item.Payment.Method.ToUpper(),
                PaymentValue = item.Payment.AmountOrdered
            });
            return payments;
        }

        private string CheckProductCode(string productCode)
        {
            if (productCode.Contains("/"))
            {
                var parts = productCode.Split('/');
                return parts[0];
            }
            return productCode;
        }
    }
}
