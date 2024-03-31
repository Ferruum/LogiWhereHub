namespace LogiWereHubSite2.Models
{
    public class WarehouseModel
    {
        public int? WarehouseId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Coordinates { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public int? CapacityId { get; set; }
    }
}
