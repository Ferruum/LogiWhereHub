namespace LogiWereHubSite2.Models
{
    public class ProductsInShipmentModel
    {
        public int? ProductInShipmentId { get; set; }
        public int? QuantityInShipment { get; set; }
        public int? ShipmentId { get; set; }
        public int? ProductId { get; set; }
        public string? NameVperevozke { get; set; }
        public decimal? Price { get; set; }
        public int? QuantityOnWarehouse { get; set; }
        public DateTime? DepartureDate { get; set; } //Дата отправления товара
    }
}
