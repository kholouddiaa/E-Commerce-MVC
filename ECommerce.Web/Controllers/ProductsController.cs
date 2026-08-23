using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.DTOs.Reviews;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Helpers;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class ProductsController(
    IProductService productService,
    ICategoryService categoryService,
    IReviewService reviewService) : Controller
{
    private const string CustomerRoleName = "Customer";

    [AllowAnonymous]
    public async Task<IActionResult> Index(ProductQueryParameters queryParameters)
    {
        var pagedProducts = await productService.GetPagedAsync(queryParameters);
        var categories = await categoryService.GetAllAsync();
        var categoryProductCounts = await productService.GetCategoryProductCountsAsync();
        var storefrontCategories = StorefrontViewModelFactory.BuildCategories(categories, categoryProductCounts);
        var normalizedSearchTerm = queryParameters.SearchTerm?.Trim() ?? string.Empty;
        var normalizedCategoryId = queryParameters.CategoryId is > 0 ? queryParameters.CategoryId : null;

        var model = new StorefrontCatalogViewModel
        {
            Products = pagedProducts.Items,
            Categories = storefrontCategories,
            SearchTerm = normalizedSearchTerm,
            CategoryId = normalizedCategoryId,
            SortOrder = ProductSortOrder.Normalize(queryParameters.SortOrder),
            CurrentPage = pagedProducts.CurrentPage,
            PageSize = pagedProducts.PageSize,
            TotalItems = pagedProducts.TotalItems,
            TotalPages = pagedProducts.TotalPages,
            SelectedCategoryName = storefrontCategories
                .FirstOrDefault(category => category.Id == normalizedCategoryId)?.Name ?? "All Categories"
        };

        return View(model);
    }

    public async Task<IActionResult> Admin()
    {
        var products = await productService.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Archived()
    {
        var products = await productService.GetArchivedAsync();
        return View(products);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var allProducts = await productService.GetAllAsync();
        var relatedProducts = BuildRelatedProducts(product, allProducts);
        var currentUserId = User.IsInRole(CustomerRoleName)
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var productReviews = await reviewService.GetProductReviewsAsync(id, currentUserId);

        var model = new StorefrontProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts,
            AverageRating = productReviews.AverageRating,
            ReviewCount = productReviews.ReviewCount,
            Reviews = productReviews.Reviews,
            CurrentUserReview = productReviews.CurrentUserReview,
            ReviewForm = BuildReviewForm(id, productReviews.CurrentUserReview)
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = await BuildProductFormViewModelAsync(new ProductUpsertDto());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategoryOptionsAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        var result = await productService.CreateAsync(viewModel.Product, viewModel.ImageFile);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(result.ErrorPropertyName ?? string.Empty, result.ErrorMessage ?? "Unable to create the product.");
            viewModel.Categories = await GetCategoryOptionsAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await productService.GetForEditAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var viewModel = await BuildProductFormViewModelAsync(product);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel viewModel)
    {
        if (id != viewModel.Product.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategoryOptionsAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        var result = await productService.UpdateAsync(viewModel.Product, viewModel.ImageFile);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(result.ErrorPropertyName ?? string.Empty, result.ErrorMessage ?? "Unable to update the product.");
            viewModel.Categories = await GetCategoryOptionsAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await productService.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await productService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to archive the product.";
            return RedirectToAction(nameof(Admin));
        }

        TempData["SuccessMessage"] = "Product archived successfully.";
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await productService.RestoreAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to restore the product.";
            return RedirectToAction(nameof(Archived));
        }

        TempData["SuccessMessage"] = "Product restored successfully.";
        return RedirectToAction(nameof(Admin));
    }

    private async Task<ProductFormViewModel> BuildProductFormViewModelAsync(ProductUpsertDto productDto)
    {
        return new ProductFormViewModel
        {
            Product = productDto,
            Categories = await GetCategoryOptionsAsync(productDto.CategoryId)
        };
    }

    private async Task<IEnumerable<ProductFormCategoryOption>> GetCategoryOptionsAsync(int selectedCategoryId = 0)
    {
        var categories = await categoryService.GetAllAsync();

        return categories.Select(category => new ProductFormCategoryOption
        {
            Value = category.Id.ToString(),
            Text = category.Name,
            Selected = category.Id == selectedCategoryId
        });
    }

    private static List<ProductDto> BuildRelatedProducts(ProductDto product, IReadOnlyList<ProductDto> allProducts)
    {
        var relatedProducts = allProducts
            .Where(candidate => candidate.Id != product.Id && candidate.CategoryId == product.CategoryId)
            .Take(4)
            .ToList();

        if (relatedProducts.Count == 0)
        {
            relatedProducts = allProducts
                .Where(candidate => candidate.Id != product.Id)
                .Take(4)
                .ToList();
        }

        return relatedProducts;
    }

    private static ReviewUpsertDto BuildReviewForm(int productId, ReviewDto? currentUserReview)
    {
        return currentUserReview is null
            ? new ReviewUpsertDto
            {
                ProductId = productId
            }
            : new ReviewUpsertDto
            {
                ProductId = productId,
                Rating = currentUserReview.Rating,
                Comment = currentUserReview.Comment
            };
    }
}
