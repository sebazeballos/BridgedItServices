namespace BridgetItService.Models.Magento
{
    public class TransactionItem
    {
        public float QtyOrdered { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public double BaseDiscountAmount { get; set; }
        public double BasePriceInclTax { get; set; }
        public double AmountRefunded { get; set; }
        public float QtyRefunded { get; set; }
    }
    
}