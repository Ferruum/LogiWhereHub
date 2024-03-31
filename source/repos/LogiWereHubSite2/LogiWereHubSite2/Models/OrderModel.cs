namespace LogiWereHubSite2.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string Coordinates { get; set; }
        public string CustomerPreferences { get; set; }
        public int UserId { get; set; }
        public int OrderTypeId { get; set; }
        public int StatusId { get; set; }
        public string UserName { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
    }
}
