namespace LogiWereHubSite2.Models
{
    public class ProductModel
    {
        public int? ProductId { get; set; }
        public decimal? Price { get; set; }
        public int? QuantityOnWarehouse { get; set; }
        public int? WarehouseId { get; set; }
        public int? DescriptionId { get; set; }
        public int? UserId { get; set; }
        public string? Name { get; set; } //Название продукта
        public string? NameWarehouse { get; set; } //Навзние склада
        public string? Address { get; set; } //Адрес склада

    }
}
