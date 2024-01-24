using System.ComponentModel.DataAnnotations;

namespace BridgetItService.Models.Database
{
    public class DBProduct
    {
        [Key]
        public string Sku { get; set; }
        public string? Name { get; set; }
        public string TypeId { get; set; }
        public int AttributeSetId { get; set; }
        public int? Visibility { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public long? Qty { get; set; }
        public bool? IsInStock { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
