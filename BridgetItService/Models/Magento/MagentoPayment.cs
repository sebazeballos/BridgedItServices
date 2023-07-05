namespace BridgetItService.Models.Magento
{
    public class MagentoPayment
    {
        public string Method { get; set; }
        public double AmountOrdered { get; set; }
        public double ShippingAmount { get; set; }
        public double AmountRefunded { get; set; }
        public double ShippingRefunded { get; set; }
    }
}