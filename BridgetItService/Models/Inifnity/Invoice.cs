﻿namespace BridgetItService.Models.Inifnity
{
    public class Invoice
    {
        public string? InvoiceCode { get; set; }
        public double invoice_value_including_sales_tax { get; set; }
        public string SalesPersonCode { get; set; }
        public int SiteCode { get; set; }
        public List<Line> Lines { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
