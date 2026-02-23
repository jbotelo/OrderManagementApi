using System.Text.Json.Serialization;

namespace Orm.Infrastructure.Cosmos;

public class OrderDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public long OrderId { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("lastModifiedDate")]
    public DateTime LastModifiedDate { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItemDocument> Items { get; set; } = [];

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("itemCount")]
    public int ItemCount { get; set; }
}

public class OrderItemDocument
{
    [JsonPropertyName("orderItemId")]
    public long OrderItemId { get; set; }

    [JsonPropertyName("productId")]
    public long ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
