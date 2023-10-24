using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;
using ShopifySharp;

namespace BridgetItService.MapperFactory
{
    public class MagentoRefundsMapping : IMap<MagentoRefund, Invoices>
    {
        public readonly string DELIVERY_SKU = "987878465789987";
        public readonly string WALLPAPER_SKU = "7777777571301";
        public readonly string FABRIC_SKU_ = "7777777571318";
        public Invoices Map(MagentoRefund refund)
        {
            var invoices = new Invoices();
            invoices.Invoice = new List<Invoice>();
            foreach (Item item in refund.Items)
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
            foreach (TransactionItem transactionItem in item.Items)
            {
                if (transactionItem.Name.Contains("Wallpaper"))
                {
                    transactionItem.Sku = WALLPAPER_SKU;
                }
                if (transactionItem.Name.Contains("Fabric"))
                {
                    transactionItem.Sku = FABRIC_SKU_;
                }
                lines.Add(new Line
                {
                    LineNumber = lineNumber++,
                    ProductCode = transactionItem.Sku,
                    Quantity = - transactionItem.QtyRefunded,
                    UnitSellingPrice =  transactionItem.BasePriceInclTax,
                    StandardUnitSellingPrice =  transactionItem.BasePriceInclTax
                });
                if (item.Payment.ShippingAmount > 0)
                {
                    lines.Add(new Line
                    {
                        LineNumber = lineNumber++,
                        ProductCode = DELIVERY_SKU,
                        Quantity = -1,
                        UnitSellingPrice = item.Payment.ShippingRefunded,
                        StandardUnitSellingPrice = item.Payment.ShippingRefunded,
                        ExtendedSalesTax = 0
                    });
                }
            }
            return lines;
        }
        private List<Payment> MapPayment(Item item)
        {
            var payments = new List<Payment>();
            var paymentLineNumber = 1;
            payments.Add(new Payment
            {
                PaymentLineNumber = paymentLineNumber++,
                TenderType = item.Payment.Method.ToUpper(),
                TenderDescription = item.Payment.Method.ToUpper(),
                PaymentValue =  -item.Payment.AmountRefunded,
            });
            return payments;
        }
    }
}
