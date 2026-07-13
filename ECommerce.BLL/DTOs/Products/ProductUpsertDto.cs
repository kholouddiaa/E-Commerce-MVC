using System.ComponentModel.DataAnnotations;

namespace ECommerce.BLL.DTOs.Products;

public class ProductUpsertDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
}
