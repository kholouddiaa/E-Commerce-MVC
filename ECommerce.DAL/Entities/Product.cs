namespace ECommerce.DAL.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
