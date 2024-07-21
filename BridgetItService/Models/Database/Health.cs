using System.ComponentModel.DataAnnotations;

namespace BridgetItService.Models.Database
{
    public class Health
    {
        [Key]
        public long Id { get; set; }
        public DateTime LastRun { get; set; }
    }
}
