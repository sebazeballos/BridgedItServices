namespace BridgetItService.Models.Inifnity
{
    public class PutProductInInfinity
    {
        public string ProductCode { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Description { get; set; }
        public decimal StandardSellingPrice { get; set; }
        public string ProductType { get; set; }
        public IList<CustomFields> CustomFields { get; set; }
    }
}
