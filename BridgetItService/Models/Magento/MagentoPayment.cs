namespace BridgetItService.Models.Magento
{
    public class MagentoPayment
    {
        public string Method { get; set; }
        public double AmountOrdered { get; set; }
        public int ShippingAmount { get; set; }
    }
}