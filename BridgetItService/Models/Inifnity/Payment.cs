namespace BridgetItService.Models.Inifnity
{
    public class Payment
    {
        public int PaymentLineNumber { get; set; }
        public string TenderType { get; set; }
        public string TenderDescription { get; set; }
        public double PaymentValue { get; set; }
    }
}