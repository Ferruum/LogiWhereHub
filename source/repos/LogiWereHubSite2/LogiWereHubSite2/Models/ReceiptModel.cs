namespace LogiWereHubSite2.Models
{
    public class ReceiptModel
    {
        public int? ReceiptId { get; set; }
        public int? OrderId { get; set; }
        public decimal? OrderAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
