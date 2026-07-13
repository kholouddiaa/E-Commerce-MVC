using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class ProductsController(IProductService productService, ICategoryService categoryService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await productService.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await productService.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
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
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        var result = await productService.CreateAsync(viewModel.Product);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create the product.");
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
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
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        var result = await productService.UpdateAsync(viewModel.Product);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update the product.");
            viewModel.Categories = await GetCategoryOptionsAsync();
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToAction(nameof(Index));
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
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to delete the product.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
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
}
