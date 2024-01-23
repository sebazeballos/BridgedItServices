namespace BridgetItService.Models.Inifnity
{
    public class GetTransactionRequestcs
    {
        public Created? Created { get; set; }
        public int Offset { get; set; }
        public string MaxRecords { get; set; }
        public int SiteCode { get; set; }
    }
}
