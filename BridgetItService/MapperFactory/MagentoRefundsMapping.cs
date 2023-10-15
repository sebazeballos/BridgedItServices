using BridgetItService.Contracts;
using BridgetItService.Models.Inifnity;
using BridgetItService.Models.Magento;

namespace BridgetItService.MapperFactory
{
    public class MagentoRefundsMapping : IMap<MagentoRefund, Invoice>
    {
        public readonly string DELIVERY_SKU = "987878465789987";
        public Invoice Map(MagentoRefund refund)
        {
            return new Invoice
            {
                SalesPersonCode = "1",
                SiteCode = "1",
                Lines = MapLines(refund),
                Payments = MapPayment(refund),
            };
        }
        private List<Line> MapLines(MagentoRefund refund)
        {
            var lines = new List<Line>();
            var lineNumber = 1;
            foreach (Item item in refund.Items)
            {
                foreach (TransactionItem transactionItem in item.Items)
                {
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
            }
            return lines;
        }
        private List<Payment> MapPayment(MagentoRefund refund)
        {
            var payments = new List<Payment>();
            var paymentLineNumber = 1;
            foreach (Item item in refund.Items)
            {
                payments.Add(new Payment
                {
                    PaymentLineNumber = paymentLineNumber++,
                    TenderType = item.Payment.Method.ToUpper(),
                    TenderDescription = item.Payment.Method.ToUpper(),
                    PaymentValue =  -item.Payment.AmountRefunded,
                });
            }
            return payments;
        }
    }
}
