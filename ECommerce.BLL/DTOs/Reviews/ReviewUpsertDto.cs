using System.ComponentModel.DataAnnotations;

namespace ECommerce.BLL.DTOs.Reviews;

public class ReviewUpsertDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Review text is required.")]
    [StringLength(1000, ErrorMessage = "Review text must be 1000 characters or fewer.")]
    [Display(Name = "Review")]
    public string Comment { get; set; } = string.Empty;
}
