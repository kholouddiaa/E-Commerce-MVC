using ECommerce.BLL.DTOs.Categories;
using ECommerce.BLL.DTOs.Products;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Helpers;

public static class StorefrontViewModelFactory
{
    public static IReadOnlyList<StoreCategoryViewModel> BuildCategories(
        IReadOnlyList<CategoryDto> categories,
        IReadOnlyList<ProductDto> products)
    {
        var productCounts = products
            .GroupBy(product => product.CategoryId)
            .ToDictionary(group => group.Key, group => group.Count());

        return BuildCategories(categories, productCounts);
    }

    public static IReadOnlyList<StoreCategoryViewModel> BuildCategories(
        IReadOnlyList<CategoryDto> categories,
        IReadOnlyDictionary<int, int> productCounts)
    {
        return categories
            .Select(category => new StoreCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = productCounts.TryGetValue(category.Id, out var count) ? count : 0,
                ImageUrl = StorefrontAssetHelper.GetCategoryImage(category.Id)
            })
            .ToList();
    }
}
