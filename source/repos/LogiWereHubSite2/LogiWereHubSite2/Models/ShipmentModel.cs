namespace LogiWereHubSite2.Models
{
    public class ShipmentModel
    {
        public int? ShipmentId { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public int? OrderId { get; set; }
        public int? VehicleId { get; set; }
        public int? StaffId { get; set; }
    }
}
