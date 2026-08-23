namespace ECommerce.BLL.DTOs.Orders;

public class OrderDetailsDto
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public IReadOnlyList<OrderItemDto> Items { get; set; } = [];
}
