namespace ECommerce.BLL.DTOs.Orders;

public class OrderItemDto
{
    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
