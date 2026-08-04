namespace ECommerce.Web.Helpers;

public static class StorefrontAssetHelper
{
    private static readonly string[] ProductImages =
    [
        "/eshopper/img/product-1.jpg",
        "/eshopper/img/product-2.jpg",
        "/eshopper/img/product-3.jpg",
        "/eshopper/img/product-4.jpg",
        "/eshopper/img/product-5.jpg",
        "/eshopper/img/product-6.jpg",
        "/eshopper/img/product-7.jpg",
        "/eshopper/img/product-8.jpg"
    ];

    private static readonly string[] CategoryImages =
    [
        "/eshopper/img/cat-1.jpg",
        "/eshopper/img/cat-2.jpg",
        "/eshopper/img/cat-3.jpg",
        "/eshopper/img/cat-4.jpg",
        "/eshopper/img/cat-5.jpg",
        "/eshopper/img/cat-6.jpg"
    ];

    public static string GetProductImage(int productId, string? imageUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            return imageUrl;
        }

        var index = GetIndex(productId, ProductImages.Length);
        return ProductImages[index];
    }

    public static string GetCategoryImage(int categoryId)
    {
        var index = GetIndex(categoryId, CategoryImages.Length);
        return CategoryImages[index];
    }

    private static int GetIndex(int id, int collectionLength)
    {
        if (collectionLength == 0)
        {
            return 0;
        }

        var normalizedId = id <= 0 ? 1 : id;
        return (normalizedId - 1) % collectionLength;
    }
}
