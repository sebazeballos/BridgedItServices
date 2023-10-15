namespace BridgetItService.Models.Magento
{
    public class TransactionItem
    {
        public int QtyOrdered { get; set; }
        public string Sku { get; set; }
        public double BasePriceInclTax { get; set; }
        public double AmountRefunded { get; set; }
        public int QtyRefunded { get; set; }
    }
    
}