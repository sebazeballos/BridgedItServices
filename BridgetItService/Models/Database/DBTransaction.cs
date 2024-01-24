using BridgetItService.Models.Inifnity;
using System.ComponentModel.DataAnnotations;

namespace BridgetItService.Models.Database
{
    public class DBTransaction
    {
        public string SalesPersonCode { get; set; }
        public string SiteCode { get; set; }
        public string MagentoTransactionId { get; set; }
        [Key]
        public string InfinityTransactionId { get; set; }
        public double PaymentValue { get; set; }
        public DateTime SentTime { get; set; }
    }
}
