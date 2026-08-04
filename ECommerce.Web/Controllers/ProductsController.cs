using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Helpers;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class ProductsController(
    IProductService productService,
    ICategoryService categoryService,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
    private const string ProductImageFolder = "uploads/products";
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId)
    {
        var allProducts = await productService.GetAllAsync();
        var categories = await categoryService.GetAllAsync();
        var storefrontCategories = StorefrontViewModelFactory.BuildCategories(categories, allProducts);

        IEnumerable<ProductDto> filteredProducts = allProducts;
        var normalizedSearchTerm = searchTerm?.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Name.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(product.Description) &&
                 product.Description.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                product.CategoryName.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (categoryId is > 0)
        {
            filteredProducts = filteredProducts.Where(product => product.CategoryId == categoryId.Value);
        }

        var model = new StorefrontCatalogViewModel
        {
            Products = filteredProducts.ToList(),
            Categories = storefrontCategories,
            SearchTerm = normalizedSearchTerm ?? string.Empty,
            CategoryId = categoryId is > 0 ? categoryId : null,
            SelectedCategoryName = storefrontCategories
                .FirstOrDefault(category => category.Id == categoryId)?.Name ?? "All Categories"
        };

        return View(model);
    }

    public async Task<IActionResult> Admin()
    {
        var products = await productService.GetAllAsync();
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
        var relatedProducts = allProducts
            .Where(candidate => candidate.Id != id && candidate.CategoryId == product.CategoryId)
            .Take(4)
            .ToList();

        if (relatedProducts.Count == 0)
        {
            relatedProducts = allProducts
                .Where(candidate => candidate.Id != id)
                .Take(4)
                .ToList();
        }

        var model = new StorefrontProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts
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
        ValidateProductImage(viewModel, existingImageUrl: null);

        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        string? uploadedImageUrl = null;
        if (viewModel.ImageFile is not null)
        {
            uploadedImageUrl = await SaveProductImageAsync(viewModel.ImageFile);
            viewModel.Product.ImageUrl = uploadedImageUrl;
        }

        var result = await productService.CreateAsync(viewModel.Product);
        if (!result.Succeeded)
        {
            DeleteProductImage(uploadedImageUrl);
            viewModel.Product.ImageUrl = null;
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create the product.");
            viewModel.Categories = await GetCategoryOptionsAsync();
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

        var currentProduct = await productService.GetByIdAsync(id);
        if (currentProduct is null)
        {
            return NotFound();
        }

        viewModel.Product.ImageUrl = currentProduct.ImageUrl;
        ValidateProductImage(viewModel, currentProduct.ImageUrl);

        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        string? uploadedImageUrl = null;
        if (viewModel.ImageFile is not null)
        {
            uploadedImageUrl = await SaveProductImageAsync(viewModel.ImageFile);
            viewModel.Product.ImageUrl = uploadedImageUrl;
        }

        var result = await productService.UpdateAsync(viewModel.Product);
        if (!result.Succeeded)
        {
            DeleteProductImage(uploadedImageUrl);
            viewModel.Product.ImageUrl = currentProduct.ImageUrl;
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update the product.");
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
        {
            DeleteProductImage(currentProduct.ImageUrl);
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
        var product = await productService.GetByIdAsync(id);
        var result = await productService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to delete the product.";
            return RedirectToAction(nameof(Admin));
        }

        DeleteProductImage(product?.ImageUrl);
        TempData["SuccessMessage"] = "Product deleted successfully.";
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

    private void ValidateProductImage(ProductFormViewModel viewModel, string? existingImageUrl)
    {
        if (viewModel.ImageFile is null)
        {
            if (string.IsNullOrWhiteSpace(existingImageUrl))
            {
                ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "Product image is required.");
            }

            return;
        }

        if (viewModel.ImageFile.Length == 0)
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "The selected image file is empty.");
        }

        if (viewModel.ImageFile.Length > MaxImageSizeInBytes)
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "The image size must be 5 MB or less.");
        }

        var fileExtension = Path.GetExtension(viewModel.ImageFile.FileName);
        if (string.IsNullOrWhiteSpace(fileExtension) ||
            !AllowedImageExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "Please upload a JPG, PNG, or WEBP image.");
        }
    }

    private async Task<string> SaveProductImageAsync(IFormFile imageFile)
    {
        Directory.CreateDirectory(GetProductImageRootPath());

        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{fileExtension}";
        var filePath = Path.Combine(GetProductImageRootPath(), fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(fileStream);

        return $"/{ProductImageFolder}/{fileName}";
    }

    private void DeleteProductImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        var relativeImagePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var candidatePath = Path.GetFullPath(Path.Combine(webRootPath, relativeImagePath));
        var productImageRootPath = Path.GetFullPath(GetProductImageRootPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!candidatePath.StartsWith(productImageRootPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (System.IO.File.Exists(candidatePath))
        {
            System.IO.File.Delete(candidatePath);
        }
    }

    private string GetProductImageRootPath()
    {
        var webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        return Path.Combine(webRootPath, "uploads", "products");
    }
}
