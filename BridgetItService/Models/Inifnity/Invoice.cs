namespace BridgetItService.Models.Inifnity
{
    public class Invoice
    {
        public string SalesPersonCode { get; set; }
        public string SiteCode { get; set; }
        public List<Line> Lines { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
