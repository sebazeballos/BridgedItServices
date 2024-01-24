﻿namespace BridgetItService.Models.Magento
{
    public class Item
    {
        //public double TotalDue { get; set; }
        public string IncrementId { get; set; }
        public double TaxAmount { get; set; }
        public double? BaseShippingRefunded { get; set; }
        public List<TransactionItem> Items { get; set; }
        public MagentoPayment Payment { get; set; }
    }
}