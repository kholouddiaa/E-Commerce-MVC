namespace ECommerce.BLL.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string ReviewerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
