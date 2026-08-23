using System.Security.Claims;
using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.DTOs.Reviews;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize(Roles = "Customer")]
public class ReviewsController(
    IProductService productService,
    IReviewService reviewService) : Controller
{
    private const string ReviewFormPrefix = nameof(StorefrontProductDetailsViewModel.ReviewForm);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = ReviewFormPrefix)] ReviewUpsertDto reviewForm)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnProductDetailsViewAsync(reviewForm.ProductId, reviewForm);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var result = await reviewService.CreateAsync(userId, reviewForm);
        if (!result.Succeeded)
        {
            AddReviewError(result);
            return await ReturnProductDetailsViewAsync(reviewForm.ProductId, reviewForm);
        }

        TempData["SuccessMessage"] = "Your review has been submitted.";
        return RedirectToAction("Details", "Products", new { id = reviewForm.ProductId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, [Bind(Prefix = ReviewFormPrefix)] ReviewUpsertDto reviewForm)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnProductDetailsViewAsync(reviewForm.ProductId, reviewForm);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var result = await reviewService.UpdateAsync(id, userId, reviewForm);
        if (!result.Succeeded)
        {
            AddReviewError(result);
            return await ReturnProductDetailsViewAsync(reviewForm.ProductId, reviewForm);
        }

        TempData["SuccessMessage"] = "Your review has been updated.";
        return RedirectToAction("Details", "Products", new { id = reviewForm.ProductId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int productId)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var result = await reviewService.DeleteAsync(id, userId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to delete your review.";
        }
        else
        {
            TempData["SuccessMessage"] = "Your review has been deleted.";
        }

        return RedirectToAction("Details", "Products", new { id = productId });
    }

    private async Task<IActionResult> ReturnProductDetailsViewAsync(int productId, ReviewUpsertDto reviewForm)
    {
        var product = await productService.GetByIdAsync(productId);
        if (product is null)
        {
            return NotFound();
        }

        var allProducts = await productService.GetAllAsync();
        var relatedProducts = BuildRelatedProducts(product, allProducts);
        var productReviews = await reviewService.GetProductReviewsAsync(productId, GetUserId());

        var model = new StorefrontProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts,
            AverageRating = productReviews.AverageRating,
            ReviewCount = productReviews.ReviewCount,
            Reviews = productReviews.Reviews,
            CurrentUserReview = productReviews.CurrentUserReview,
            ReviewForm = reviewForm
        };

        return View("~/Views/Products/Details.cshtml", model);
    }

    private void AddReviewError(ECommerce.BLL.Common.OperationResult result)
    {
        if (string.IsNullOrWhiteSpace(result.ErrorPropertyName))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to save your review.");
            return;
        }

        ModelState.AddModelError(
            $"{ReviewFormPrefix}.{result.ErrorPropertyName}",
            result.ErrorMessage ?? "Unable to save your review.");
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
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
}
