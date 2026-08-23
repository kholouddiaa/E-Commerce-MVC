namespace ECommerce.DAL.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
