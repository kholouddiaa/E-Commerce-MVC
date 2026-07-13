using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class ProductFormViewModel
{
    public ProductUpsertDto Product { get; set; } = new();

    public IEnumerable<ProductFormCategoryOption> Categories { get; set; } = Enumerable.Empty<ProductFormCategoryOption>();

    public bool HasCategories => Categories.Any();
}
