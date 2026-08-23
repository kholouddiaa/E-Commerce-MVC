namespace ECommerce.DAL.Entities;

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime OrderDate { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
