using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;
using ShopifySharp;

namespace BridgetItService.MapperFactory
{
    public class MagentoTransactionsMap : IMap<MagentoOrder, Invoice>
    {
        public readonly string DELIVERY_SKU = "987878465789987";
        public Invoice Map(MagentoOrder order)
        {
            return new Invoice
            {
                SalesPersonCode = "1",
                SiteCode = "909",
                Lines = MapLines(order),
                Payments = MapPayment(order),
            };
        }
        private List<Line> MapLines(MagentoOrder order)
        {
            var lines = new List<Line>();
            var lineNumber = 1;
            foreach(Item item in order.Items)
            {
                foreach(TransactionItem transactionItem in item.Items)
                {
                    lines.Add(new Line
                    {
                        LineNumber = lineNumber++,
                        ProductCode = transactionItem.Sku,
                        Quantity = transactionItem.QtyOrdered,
                        UnitSellingPrice = transactionItem.BasePrice,
                        StandardUnitSellingPrice = transactionItem.BasePrice,
                        ExtendedSalesTax = item.TaxAmount
                    });
                    if (item.Payment.ShippingAmount > 0) {
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
                }
            }
            return lines;
        }
        private List<Payment> MapPayment(MagentoOrder order)
        {
            var payments = new List<Payment>();
            var paymentLineNumber = 1;
            foreach(Item item in order.Items)
            {
                payments.Add(new Payment
                {
                    PaymentLineNumber = paymentLineNumber++,
                    TenderType = item.Payment.Method,
                    TenderDescription = item.Payment.Method,
                    PaymentValue = item.Payment.AmountOrdered
                });
            }
            return payments;
        }
    }
}
