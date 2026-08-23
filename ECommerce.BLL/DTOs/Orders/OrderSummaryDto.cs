namespace ECommerce.BLL.DTOs.Orders;

public class OrderSummaryDto
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }
}
