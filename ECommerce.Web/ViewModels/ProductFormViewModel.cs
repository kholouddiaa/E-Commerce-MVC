using System.ComponentModel.DataAnnotations;
using ECommerce.BLL.DTOs.Products;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Web.ViewModels;

public class ProductFormViewModel
{
    public ProductUpsertDto Product { get; set; } = new();

    [Display(Name = "Product Image")]
    public IFormFile? ImageFile { get; set; }

    public IEnumerable<ProductFormCategoryOption> Categories { get; set; } = Enumerable.Empty<ProductFormCategoryOption>();

    public bool HasCategories => Categories.Any();

    public bool RequiresImage => string.IsNullOrWhiteSpace(Product.ImageUrl);
}
